using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Visiter;
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
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если <paramref name="elementToHighlight"/> не стена или перекрытие</exception>
    public static ICollection<ParameterFilterElement> GetHighlightFilters(Document doc, Element elementToHighlight) {
        if(elementToHighlight is null) { throw new ArgumentNullException(nameof(elementToHighlight)); }

        if(elementToHighlight is Wall wall) {
            var wallFilter = GetWallHighlightFilter(doc, wall);
            var floorFilter = GetFloorHighlightFilter(doc);
            return new ParameterFilterElement[] { wallFilter, floorFilter };

        } else if(elementToHighlight is Floor floor) {
            var wallFilter = GetWallHighlightFilter(doc);
            var floorFilter = GetFloorHighlightFilter(doc, floor);
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
    /// <param name="wall">Заданная стена, которая не проходит фильтр</param>
    private static ParameterFilterElement GetWallHighlightFilter(Document doc, Wall wall = null) {
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
                wallsFilter.SetElementFilter(GetHighlightElementFilter(wall));
            }
            t.Commit();
        }
        return wallsFilter;
    }

    /// <summary>
    /// Возвращает фильтр по перекрытиям, которые не являются заданным перекрытием
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="floor">Заданное перекрытие</param>
    private static ParameterFilterElement GetFloorHighlightFilter(Document doc, Floor floor = null) {
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
                floorsFilter.SetElementFilter(GetHighlightElementFilter(floor));
            }
            t.Commit();
        }
        return floorsFilter;
    }

    /// <summary>
    /// Создает фильтр, в который попадают все стены, кроме заданной
    /// </summary>
    /// <param name="wallToHighlight">Стена, которая не должна проходить фильтр</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private static ElementFilter GetHighlightElementFilter(Wall wallToHighlight) {
        if(wallToHighlight is null) { throw new ArgumentNullException(nameof(wallToHighlight)); }

        // имя типа стены
        var typeNameParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
        string wallTypeName = wallToHighlight.WallType.Name;

        // отсеиваем все стены, у которых название типа не равно заданному
        var typeNameNotEqualsRule = new NotEqualsVisister().Create(new ElementId(typeNameParam), wallTypeName);
        var typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);


        // длина стены
        var lengthParam = BuiltInParameter.CURVE_ELEM_LENGTH;
        double wallLength = wallToHighlight.GetParamValue<double>(lengthParam);
        var lengthParamId = new ElementId(lengthParam);

        // отсеиваем все стены, длина которых меньше заданной
        var lengthLessRule = new LessVisister().Create(lengthParamId, wallLength);
        var lengthLessFilter = new ElementParameterFilter(lengthLessRule);

        // отсеиваем все стены, длина которых больше заданной
        var lengthGreaterRule = new GreaterVisister().Create(lengthParamId, wallLength);
        var lengthGreaterFilter = new ElementParameterFilter(lengthGreaterRule);

        return new LogicalOrFilter([
            typeNameNotEqualsFilter,
            lengthLessFilter,
            lengthGreaterFilter
        ]);
    }

    /// <summary>
    /// Создает фильтр, в который попадают все перекрытия, кроме заданного
    /// </summary>
    /// <param name="floorToHighlight">Перекрытие, которое не должно проходить фильтр</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private static ElementFilter GetHighlightElementFilter(Floor floorToHighlight) {
        if(floorToHighlight is null) { throw new ArgumentNullException(nameof(floorToHighlight)); }

        // имя типа перекрытия
        var typeNameParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
        string floorTypeName = floorToHighlight.FloorType.Name;

        // отсеиваем все перекрытия, у которых название типа не равно заданному
        var typeNameNotEqualsRule = new NotEqualsVisister().Create(new ElementId(typeNameParam), floorTypeName);
        var typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);


        // периметр перекрытия
        var perimeterParam = BuiltInParameter.HOST_PERIMETER_COMPUTED;
        double floorPerimeter = floorToHighlight.GetParamValue<double>(perimeterParam);
        var perimeterParamId = new ElementId(perimeterParam);

        // отсеиваем все перекрытия, периметр которых меньше заданного
        var perimeterLessRule = new LessVisister().Create(perimeterParamId, floorPerimeter);
        var perimeterLessFilter = new ElementParameterFilter(perimeterLessRule);

        // отсеиваем все перекрытия, периметр которых больше заданного
        var perimeterGreaterRule = new GreaterVisister().Create(perimeterParamId, floorPerimeter);
        var perimeterGreaterFilter = new ElementParameterFilter(perimeterGreaterRule);

        return new LogicalOrFilter([
            typeNameNotEqualsFilter,
            perimeterLessFilter,
            perimeterGreaterFilter
        ]);
    }
}
