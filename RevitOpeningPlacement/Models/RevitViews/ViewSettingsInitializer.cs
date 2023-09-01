using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.Models.RevitViewSettings;
using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.RevitViews.RevitViewSettings;

namespace RevitOpeningPlacement.Models.RevitViews {
    internal class ViewSettingsInitializer {
        public static List<IView3DSetting> GetView3DSettings(Document doc) {
            return new List<IView3DSetting>() {
                GetDetailLevelSetting(),
                GetDisplayStyleSetting(),
                GetHiddenCategoriesSetting(),
                GetOpeningFilterSetting(doc),
                GetMepFilterSetting(doc),
                GetConstructureFilterSetting(doc),
                GetViewDisplayModelSetting(),
                GetParamSetting(doc),
                GetVisibleModelCategoriesSetting(),
                GetDisciplineSetting(),
            }.Where(item => item != null)
            .ToList();
        }

        /// <summary>
        /// Настройки уровня детализации
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetDetailLevelSetting() {
            return new DetailLevelSetting(ViewDetailLevel.Fine);
        }

        /// <summary>
        /// Настройки визуального стиля
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetDisplayStyleSetting() {
            return new DisplayStyleSetting(DisplayStyle.ShadingWithEdges);
        }

        /// <summary>
        /// Настройки дисциплины
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetDisciplineSetting() {
            return new DisciplineSetting();
        }

        /// <summary>
        /// Настройки видимости категорий модели
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetVisibleModelCategoriesSetting() {
            var categories = new HashSet<BuiltInCategory>();
            categories.UnionWith(FiltersInitializer.GetAllUsedMepCategories());
            categories.UnionWith(FiltersInitializer.GetAllUsedStructureCategories());
            categories.UnionWith(FiltersInitializer.GetAllUsedOpeningsCategories());
            categories.UnionWith(new BuiltInCategory[] {
                BuiltInCategory.OST_RvtLinks,
                BuiltInCategory.OST_GenericModel ,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Ceilings,
                BuiltInCategory.OST_PlumbingFixtures
            });
            return new VisibleModelCategoriesSetting(categories);
        }

        /// <summary>
        /// Настройки видимости категорий аннотаций
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetHiddenCategoriesSetting() {
            return new HiddenCategoriesSetting(new BuiltInCategory[] {
                BuiltInCategory.OST_Levels,
                BuiltInCategory.OST_WallRefPlanes,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_VolumeOfInterest
            });
        }

        /// <summary>
        /// Фильтр видимости для заданий на отверстия
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IView3DSetting GetOpeningFilterSetting(Document doc) {
            var filter = ParameterFilterInitializer.GetOpeningFilter(doc);
            var graphicSettings = GraphicSettingsInitializer.GetOpeningGraphicSettings(doc);
            return new FilterSetting(filter, graphicSettings);
        }

        /// <summary>
        /// Фильтр видимости для инженерных элементов
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IView3DSetting GetMepFilterSetting(Document doc) {
            var filter = ParameterFilterInitializer.GetMepFilter(doc);
            var graphicSettings = GraphicSettingsInitializer.GetMepGraphicSettings(doc);
            return new FilterSetting(filter, graphicSettings);
        }

        /// <summary>
        /// Фильтр видимости для элементов конструкций
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IView3DSetting GetConstructureFilterSetting(Document doc) {
            var filter = ParameterFilterInitializer.GetConstructureFilter(doc);
            var graphicSettings = GraphicSettingsInitializer.GetConstructureGraphicSettings(doc);
            return new FilterSetting(filter, graphicSettings);
        }

        /// <summary>
        /// Прозрачность
        /// </summary>
        /// <returns></returns>
        private static IView3DSetting GetViewDisplayModelSetting() {
            return new TransparencySetting(1);
        }

        private static IView3DSetting GetParamSetting(Document doc) {
            var bimGroup = new FilteredElementCollector(doc)
                        .OfClass(typeof(View3D))
                        .Cast<View3D>()
                        .Where(item => !item.IsTemplate)
                        .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
                        .FirstOrDefault(item => item != null && item.Contains("BIM"));
            if(bimGroup != null) {
                return new ParamSetting(ProjectParamsConfig.Instance.ViewGroup.Name, new StringParamValue(bimGroup));
            } else {
                return null;
            }
        }
    }
}
