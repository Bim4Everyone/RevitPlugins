using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.RevitViews;
using RevitClashDetective.Models.RevitViewSettings;
using RevitClashDetective.Models.Value;

using RevitSleeves.Models.Config;
using RevitSleeves.Services.ViewSettings;

namespace RevitSleeves.Services.Core;
internal class SleeveView3dProvider : IView3DProvider {
    private readonly SleevePlacementSettingsConfig _config;
    private readonly ILocalizationService _localizationService;

    public SleeveView3dProvider(
        SleevePlacementSettingsConfig config,
        ILocalizationService localizationService) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public View3D GetView(Document doc, string name) {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(View3D))
            .OfType<View3D>()
            .FirstOrDefault(v => !v.IsTemplate && v.Name.Equals(name))
            ?? new View3DBuilder()
            .SetName(name)
            .SetViewSettings(GetSettings(doc))
            .Build(doc);
    }

    private IView3DSetting[] GetSettings(Document doc) {
        return [.. new List<IView3DSetting>() {
                GetDetailLevelSetting(),
                GetDisplayStyleSetting(),
                GetHiddenCategoriesSetting(),
                GetSleeveFilterSetting(doc),
                GetMepFilterSetting(doc),
                GetConstructureFilterSetting(doc),
                GetParamSetting(doc),
                GetSecondaryCategoriesSetting(doc),
                GetDisciplineSetting(),
            }.Where(item => item != null)];
    }


    private HashSet<BuiltInCategory> GetAllModelCategories(Document document) {
        var allCategories = document.Settings.Categories;
        var modelCategories = new HashSet<BuiltInCategory>();
        foreach(Category category in allCategories) {
            if(category.CategoryType == CategoryType.Model) {
                modelCategories.Add(category.GetBuiltInCategory());
            }
        }
        return modelCategories;
    }

    /// <summary>
    /// Возвращает первый существующий фильтр из документа с заданным именем, или создает его и также возвращает
    /// </summary>
    private ParameterFilterElement CreateFilter(
        Document doc,
        string filterName,
        ICollection<BuiltInCategory> categories,
        ICollection<FilterRule> filterRules) {
        var filter = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterFilterElement))
            .OfType<ParameterFilterElement>()
            .FirstOrDefault(item => item.Name.Equals(filterName));
        if(filter == null) {
            using var t = doc.StartTransaction(_localizationService.GetLocalizedString("Transaction.CreateFilter"));
            filter = ParameterFilterElement.Create(doc, filterName, [.. categories.Select(item => new ElementId(item))]);
            if(filterRules.Any()) {
                var logicalAndFilter = new LogicalAndFilter(
                    [.. filterRules.Select(item => new ElementParameterFilter(item))]);
                filter.SetElementFilter(logicalAndFilter);
            }
            t.Commit();
        }
        return filter;
    }

    /// <summary>
    /// Настройки уровня детализации
    /// </summary>
    private IView3DSetting GetDetailLevelSetting() {
        return new DetailLevelSetting(ViewDetailLevel.Fine);
    }

    /// <summary>
    /// Настройки визуального стиля
    /// </summary>
    private IView3DSetting GetDisplayStyleSetting() {
        return new DisplayStyleSetting(DisplayStyle.FlatColors);
    }

    /// <summary>
    /// Настройки дисциплины
    /// </summary>
    private IView3DSetting GetDisciplineSetting() {
        return new CoordinationDisciplineSetting();
    }

    /// <summary>
    /// Настройки видимости категорий модели, не являющихся важными
    /// </summary>
    private IView3DSetting GetSecondaryCategoriesSetting(Document doc) {
        var filter = GetSecondaryCategoriesFilter(doc);
        var graphicSettings = GetSecondaryElementsGraphicSettings();
        return new FilterSetting(filter, graphicSettings);
    }

    private OverrideGraphicSettings GetSecondaryElementsGraphicSettings() {
        return new OverrideGraphicSettings().SetHalftone(true).SetSurfaceTransparency(70);
    }

    private ParameterFilterElement GetSecondaryCategoriesFilter(Document doc) {
        // все категории, которые не должны попасть в неинтересные
        var mainCategories = new HashSet<BuiltInCategory>();
        mainCategories.UnionWith(GetAllUsedMepCategories());
        mainCategories.UnionWith(GetAllUsedStructureCategories());
        mainCategories.UnionWith([BuiltInCategory.OST_RvtLinks]);

        var secondaryCategories = GetAllModelCategories(doc);
        secondaryCategories.ExceptWith(mainCategories);

        return CreateFilter(doc,
            NamesProvider.FilterNameSecondaryCategories,
            secondaryCategories,
            []);
    }

    /// <summary>
    /// Скрытие ненужных категорий
    /// </summary>
    private IView3DSetting GetHiddenCategoriesSetting() {
        return new HiddenCategoriesSetting([
                BuiltInCategory.OST_Levels,
                BuiltInCategory.OST_WallRefPlanes,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_VolumeOfInterest,

                //все, что касается арматуры
                BuiltInCategory.OST_Coupler,
                BuiltInCategory.OST_FabricReinforcement,
                BuiltInCategory.OST_FabricReinforcementWire,
                BuiltInCategory.OST_FabricAreas,
                BuiltInCategory.OST_PathRein,
                BuiltInCategory.OST_Cage,
                BuiltInCategory.OST_AreaRein,
                BuiltInCategory.OST_Rebar,
                BuiltInCategory.OST_StructuralTendons
            ]);
    }

    /// <summary>
    /// Фильтр видимости для гильз
    /// </summary>
    private IView3DSetting GetSleeveFilterSetting(Document doc) {
        var filter = GetSleeveFilter(doc);
        var graphicSettings = GetSleeveGraphicSettings(doc);
        return new FilterSetting(filter, graphicSettings);
    }

    private ParameterFilterElement GetSleeveFilter(Document doc) {
        var category = Category.GetCategory(doc, SleevePlacementSettingsConfig.SleeveCategory);
        var nameParameter = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, [category.Id])
            .First(item => item.IsSystemId() && item.AsBuiltInParameter() == BuiltInParameter.ALL_MODEL_FAMILY_NAME);
        string famName = NamesProvider.FamilyNameSleeve;
#if REVIT_2022_OR_LESS
        var filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, famName, false);
#else
        var filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, famName);
#endif
        return CreateFilter(
            doc,
            NamesProvider.FilterNameSleeves,
            [SleevePlacementSettingsConfig.SleeveCategory],
            [filterRule]);
    }

    private OverrideGraphicSettings GetSleeveGraphicSettings(Document doc) {
        // оранжевый
        return GetGraphicSettings(doc, new Color(255, 165, 0));
    }

    private OverrideGraphicSettings GetGraphicSettings(Document doc, Color color) {
        var settings = new OverrideGraphicSettings();

        settings
            .SetCutBackgroundPatternColor(color)
            .SetCutForegroundPatternColor(color)
            .SetSurfaceBackgroundPatternColor(color)
            .SetSurfaceForegroundPatternColor(color);

        var solidFillPattern = new FilteredElementCollector(doc)
            .OfClass(typeof(FillPatternElement))
            .OfType<FillPatternElement>()
            .FirstOrDefault(item => item.GetFillPattern().IsSolidFill);
        if(solidFillPattern != null) {
            settings
                .SetSurfaceBackgroundPatternId(solidFillPattern.Id)
                .SetSurfaceForegroundPatternId(solidFillPattern.Id);
        }
        return settings;
    }

    /// <summary>
    /// Фильтр видимости для инженерных элементов
    /// </summary>
    private IView3DSetting GetMepFilterSetting(Document doc) {
        var filter = GetMepFilter(doc);
        var graphicSettings = GetMepGraphicSettings(doc);
        return new FilterSetting(filter, graphicSettings);
    }

    private ParameterFilterElement GetMepFilter(Document doc) {
        return CreateFilter(doc,
            NamesProvider.FilterNameMep,
            GetAllUsedMepCategories(),
            []);
    }

    private ICollection<BuiltInCategory> GetAllUsedMepCategories() {
        return [_config.PipeSettings.Category, SleevePlacementSettingsConfig.SleeveCategory];
    }

    private OverrideGraphicSettings GetMepGraphicSettings(Document doc) {
        // зеленый
        return GetGraphicSettings(doc, new Color(0, 128, 0)).SetSurfaceTransparency(10);
    }

    /// <summary>
    /// Фильтр видимости для элементов конструкций
    /// </summary>
    private IView3DSetting GetConstructureFilterSetting(Document doc) {
        var filter = GetConstructureFilter(doc);
        var graphicSettings = GetConstructureGraphicSettings(doc);
        return new FilterSetting(filter, graphicSettings);
    }

    private ParameterFilterElement GetConstructureFilter(Document doc) {
        return CreateFilter(doc,
            NamesProvider.FilterNameConstructions,
            GetAllUsedStructureCategories(),
            []);
    }

    private ICollection<BuiltInCategory> GetAllUsedStructureCategories() {
        return [_config.PipeSettings.FloorSettings.Category, _config.PipeSettings.WallSettings.Category];
    }

    private OverrideGraphicSettings GetConstructureGraphicSettings(Document doc) {
        // серый
        return GetGraphicSettings(doc, new Color(128, 128, 128));
    }

    /// <summary>
    /// Настройка значений параметров 3D вида
    /// </summary>
    private IView3DSetting GetParamSetting(Document doc) {
        string bimGroup = new FilteredElementCollector(doc)
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .Where(item => !item.IsTemplate)
            .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
            .FirstOrDefault(item => item != null && item.Contains(NamesProvider.BIM));
        if(bimGroup != null) {
            return new ParamSetting(ProjectParamsConfig.Instance.ViewGroup.Name, new StringParamValue(bimGroup));
        } else {
            return null;
        }
    }
}
