using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;

using dosymep.Revit;

using RevitOpeningPlacement.Models.Filtration;

namespace RevitOpeningPlacement.Models.RevitViews;
internal class ParameterFilterInitializer {
    /// <summary>
    /// Возвращает фильтр по заданиям на отверстия
    /// </summary>
    public static ParameterFilterElement GetOpeningFilter(Document doc) {
        var category = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
        var nameParameter = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, new[] { category.Id })
            .FirstOrDefault(item => item.IsSystemId() && (BuiltInParameter) item.GetIdValue() == BuiltInParameter.ALL_MODEL_FAMILY_NAME);
        FilterRule filterRule = default;
        if(nameParameter != null) {
            string famName = "ОбщМд_Отв";
#if REVIT_2022_OR_LESS
            filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, famName, false);
#else
            filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, famName);
#endif
        }
        if(filterRule == null) {
            // такого не может быть, но вдруг
            throw new ArgumentException("Отсутствует параметр \"Имя семейства\".", nameof(nameParameter));
        }
        return CreateFilter(doc, "BIM_Отверстия", new[] { BuiltInCategory.OST_GenericModel }, new[] { filterRule });
    }

    /// <summary>
    /// Возвращает фильтр по всем категориям инженерных систем, использующимся для расстановки заданий на отверстия
    /// </summary>
    public static ParameterFilterElement GetMepFilter(Document doc) {
        return CreateFilter(doc,
            "BIM_Инж_Системы",
            FiltersInitializer.GetAllUsedMepCategories(),
            new FilterRule[] { });
    }

    /// <summary>
    /// Возвращает фильтр по всем категориям конструкций (стены, перекрытия)
    /// </summary>
    public static ParameterFilterElement GetConstructureFilter(Document doc) {
        return CreateFilter(doc,
            "BIM_Конструкции",
            FiltersInitializer.GetAllUsedStructureCategories(),
            new FilterRule[] { });
    }

    /// <summary>
    /// Возвращает фильтр по всем неинтересным категориям для работы с заданиями на отверстия
    /// </summary>
    public static ParameterFilterElement GetSecondaryCategoriesFilter(Document doc) {
        // все категории, которые не должны попасть в не интересные
        var mainCategories = new HashSet<BuiltInCategory>();
        mainCategories.UnionWith(FiltersInitializer.GetAllUsedMepCategories());
        mainCategories.UnionWith(FiltersInitializer.GetAllUsedStructureCategories());
        mainCategories.UnionWith(FiltersInitializer.GetAllUsedOpeningsCategories());
        mainCategories.UnionWith(new BuiltInCategory[] {
            BuiltInCategory.OST_RvtLinks,
            BuiltInCategory.OST_GenericModel
        });

        var secondaryCategories = GetAllModelCategories(doc);
        secondaryCategories.ExceptWith(mainCategories);

        return CreateFilter(doc,
            "BIM_Вспомогательные_категории",
            secondaryCategories,
            new FilterRule[] { });
    }

    /// <summary>
    /// Возвращает коллекцию фильтров для вида, в которые попадают все конструкции (стены и перекрытия), которые не являются заданной стеной или перекрытием
    /// <para>Если фильтры с такими названиями уже есть в документе, они будут изменены</para>
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация элементов</param>
    /// <param name="elementToHighlight">Элемент который нужно выделить</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если <paramref name="elementToHighlight"/> не стена или перекрытие</exception>
    public static ICollection<ParameterFilterElement> GetHighlightFilters(
        Document doc,
        Element elementToHighlight,
        ILogicalFilterFactory filterFactory) {
        if(elementToHighlight is null) { throw new ArgumentNullException(nameof(elementToHighlight)); }
        if(filterFactory is null) { throw new ArgumentNullException(nameof(filterFactory)); }

        if(elementToHighlight is Wall wall) {
            var wallFilter = GetWallHighlightFilter(doc, filterFactory, wall);
            var floorFilter = GetFloorHighlightFilter(doc, filterFactory);
            return new ParameterFilterElement[] { wallFilter, floorFilter };

        } else if(elementToHighlight is Floor floor) {
            var wallFilter = GetWallHighlightFilter(doc, filterFactory);
            var floorFilter = GetFloorHighlightFilter(doc, filterFactory, floor);
            return new ParameterFilterElement[] { wallFilter, floorFilter };

        } else {
            throw new ArgumentException(nameof(elementToHighlight));
        }
    }

    /// <summary>
    /// Возвращает первый существующий фильтр из документа с заданным именем, или создает его и также возвращает
    /// </summary>
    private static ParameterFilterElement CreateFilter(Document doc, string name, ICollection<BuiltInCategory> categories, ICollection<FilterRule> filterRules) {
        var filter = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterFilterElement))
            .OfType<ParameterFilterElement>()
            .FirstOrDefault(item => item.Name.Equals(name));
        if(filter == null) {
            using var t = doc.StartTransaction("Создание фильтра");
            filter = ParameterFilterElement.Create(doc, name, categories.Select(item => new ElementId(item)).ToArray());
            if(filterRules.Any()) {
                var logicalAndFilter = new LogicalAndFilter(filterRules.Select(item => new ElementParameterFilter(item)).ToArray());
                filter.SetElementFilter(logicalAndFilter);
            }

            t.Commit();
        }

        return filter;
    }

    /// <summary>
    /// Возвращает все категории модели из документа
    /// </summary>
    /// <param name="document">Документ с категориями</param>
    private static HashSet<BuiltInCategory> GetAllModelCategories(Document document) {
        var allCategories = document.Settings.Categories;
        HashSet<BuiltInCategory> modelCategories = [];
        foreach(Category category in allCategories) {
            if(category.CategoryType == CategoryType.Model) {
                modelCategories.Add(category.GetBuiltInCategory());
            }
        }
        return modelCategories;
    }

    /// <summary>
    /// Возвращает фильтр по стенам, которые не являются заданной стеной
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <param name="wall">Заданная стена, которая не проходит фильтр</param>
    private static ParameterFilterElement GetWallHighlightFilter(
        Document doc,
        ILogicalFilterFactory filterFactory,
        Wall wall = null) {
        var wallsFilter = CreateFilter(doc,
            $"BIM_Стены_НЕ_Хост_Отверстия_{doc.Application.Username}",
            new BuiltInCategory[] { RevitRepository.WallCategory },
            new FilterRule[] { }
            );
        using(var t = doc.StartTransaction("Обновление фильтра стен")) {
            // переназначить категории элементов, если пользователь изменил их
            wallsFilter.SetCategories(new ElementId[] { new(RevitRepository.WallCategory) });
            // сбросить все существующие критерии фильтрации
            wallsFilter.ClearRules();
            if(wall != null) {
                wallsFilter.SetElementFilter(GetHighlightElementFilter(doc, filterFactory, wall));
            }
            t.Commit();
        }
        return wallsFilter;
    }

    /// <summary>
    /// Возвращает фильтр по перекрытиям, которые не являются заданным перекрытием
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <param name="floor">Заданное перекрытие</param>
    private static ParameterFilterElement GetFloorHighlightFilter(
        Document doc,
        ILogicalFilterFactory filterFactory,
        Floor floor = null) {
        var floorsFilter = CreateFilter(doc,
            $"BIM_Перекрытия_НЕ_Хост_Отверстия_{doc.Application.Username}",
            new BuiltInCategory[] { RevitRepository.FloorCategory },
            new FilterRule[] { }
            );
        using(var t = doc.StartTransaction("Обновление фильтра перекрытий")) {
            // переназначить категории элементов, если пользователь изменил их
            floorsFilter.SetCategories(new ElementId[] { new(RevitRepository.FloorCategory) });
            // сбросить все существующие критерии фильтрации
            floorsFilter.ClearRules();
            if(floor != null) {
                floorsFilter.SetElementFilter(GetHighlightElementFilter(doc, filterFactory, floor));
            }
            t.Commit();
        }
        return floorsFilter;
    }

    /// <summary>
    /// Создает фильтр, в который попадают все стены, кроме заданной
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <param name="wallToHighlight">Стена, которая не должна проходить фильтр</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private static ElementFilter GetHighlightElementFilter(
        Document doc,
        ILogicalFilterFactory filterFactory,
        Wall wallToHighlight) {
        if(wallToHighlight is null) { throw new ArgumentNullException(nameof(wallToHighlight)); }

        double wallLength = wallToHighlight.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH);

        var filter = filterFactory.CreateOrFilter();
        // отсеиваем все стены, у которых название типа не равно заданному
        filter.AddNotEqualsRule(BuiltInParameter.ALL_MODEL_TYPE_NAME, wallToHighlight.WallType.Name);
        // отсеиваем все стены, длина которых меньше заданной
        filter.AddLessRule(BuiltInParameter.CURVE_ELEM_LENGTH, wallLength);
        // отсеиваем все стены, длина которых больше заданной
        filter.AddGreaterRule(BuiltInParameter.CURVE_ELEM_LENGTH, wallLength);

        return filter.Build(doc, FilterBuildOptions.Create());
    }

    /// <summary>
    /// Создает фильтр, в который попадают все перекрытия, кроме заданного
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="filterFactory">Фабрика фильтров элементов</param>
    /// <param name="floorToHighlight">Перекрытие, которое не должно проходить фильтр</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private static ElementFilter GetHighlightElementFilter(
        Document doc,
        ILogicalFilterFactory filterFactory,
        Floor floorToHighlight) {
        if(floorToHighlight is null) { throw new ArgumentNullException(nameof(floorToHighlight)); }

        double floorPerimeter = floorToHighlight.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED);

        var filter = filterFactory.CreateOrFilter();
        // отсеиваем все перекрытия, у которых название типа не равно заданному
        filter.AddNotEqualsRule(BuiltInParameter.ALL_MODEL_TYPE_NAME, floorToHighlight.FloorType.Name);
        // отсеиваем все перекрытия, периметр которых меньше заданного
        filter.AddLessRule(BuiltInParameter.HOST_PERIMETER_COMPUTED, floorPerimeter);
        // отсеиваем все перекрытия, периметр которых больше заданного
        filter.AddGreaterRule(BuiltInParameter.HOST_PERIMETER_COMPUTED, floorPerimeter);

        return filter.Build(doc, FilterBuildOptions.Create());
    }
}
