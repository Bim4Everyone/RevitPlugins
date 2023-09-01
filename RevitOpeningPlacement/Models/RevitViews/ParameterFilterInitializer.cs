using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Visiter;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class ParameterFilterInitializer {
        public static ParameterFilterElement GetOpeningFilter(Document doc) {
            var category = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
            var nameParameter = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, new[] { category.Id })
                .FirstOrDefault(item => item.IsSystemId() && (BuiltInParameter) item.IntegerValue == BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            FilterRule filterRule = default;
            if(nameParameter != null) {
                filterRule = ParameterFilterRuleFactory.CreateBeginsWithRule(nameParameter, "ОбщМд_Отв", false);
            }
            if(filterRule == null) {
                throw new ArgumentException("Отсутствует параметр \"Имя семейства\".", nameof(nameParameter));
            }
            return CreateFilter(doc, "BIM_Отверстия", new[] { BuiltInCategory.OST_GenericModel }, new[] { filterRule });
        }

        public static ParameterFilterElement GetMepFilter(Document doc) {
            return CreateFilter(doc,
                "BIM_Инж_Системы",
                FiltersInitializer.GetAllUsedMepCategories(),
                new FilterRule[] { });
        }

        public static ParameterFilterElement GetConstructureFilter(Document doc) {
            return CreateFilter(doc,
                "BIM_Конструкции",
                FiltersInitializer.GetAllUsedStructureCategories(),
                new FilterRule[] { });
        }

        /// <summary>
        /// Возвращает коллекцию фильтров для вида, в которые попадают все конструкции (стены и перекрытия), которые не являются заданной стеной или перекрытием
        /// <para>Если фильтры с такими названиями уже есть в документе, они будут изменены</para>
        /// </summary>
        /// <param name="doc">Документ, в котором происходит фильтрация элементов</param>
        /// <param name="elementToHighlight"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Исключение, если <paramref name="elementToHighlight"/> не стена или перекрытие</exception>
        public static ICollection<ParameterFilterElement> GetHighlightFilters(Document doc, Element elementToHighlight) {
            if(elementToHighlight is null) { throw new ArgumentNullException(nameof(elementToHighlight)); }

            // TODO исправить баг с не обновлением графики для элемента, для которого создавался предыдущий фильтр
            // возможно поможет после вызова ClearRules добавить правило, чтобы ему удовлетворяли все стены или все перекрытия
            // например, периметр больше 0 и длина больше 0
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
        /// <param name="doc"></param>
        /// <param name="name"></param>
        /// <param name="categories"></param>
        /// <param name="filterRules"></param>
        /// <returns></returns>
        private static ParameterFilterElement CreateFilter(Document doc, string name, ICollection<BuiltInCategory> categories, ICollection<FilterRule> filterRules) {
            ParameterFilterElement filter = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .FirstOrDefault(item => item.Name.Equals(name));
            if(filter == null) {
                using(Transaction t = doc.StartTransaction("Создание фильтра")) {
                    filter = ParameterFilterElement.Create(doc, name, categories.Select(item => new ElementId(item)).ToArray());
                    if(filterRules.Any()) {
                        var logicalAndFilter = new LogicalAndFilter(filterRules.Select(item => new ElementParameterFilter(item)).ToArray());
                        filter.SetElementFilter(logicalAndFilter);
                    }

                    t.Commit();
                }
            }

            return filter;
        }

        private static ParameterFilterElement GetWallHighlightFilter(Document doc, Wall wall = null) {
            var wallsFilter = CreateFilter(doc,
                $"BIM_Стены_НЕ_Хост_Отверстия_{doc.Application.Username}",
                new BuiltInCategory[] { BuiltInCategory.OST_Walls },
                new FilterRule[] { }
                );
            using(Transaction t = doc.StartTransaction("Обновление фильтра стен")) {
                // переназначить категории элементов, если пользователь изменил их
                wallsFilter.SetCategories(new ElementId[] { new ElementId(BuiltInCategory.OST_Walls) });
                // сбросить все существующие критерии фильтрации
                wallsFilter.ClearRules();
                if(wall != null) {
                    wallsFilter.SetElementFilter(GetHighlightElementFilter(wall));
                }
                t.Commit();
            }
            return wallsFilter;
        }

        private static ParameterFilterElement GetFloorHighlightFilter(Document doc, Floor floor = null) {
            var floorsFilter = CreateFilter(doc,
                $"BIM_Перекрытия_НЕ_Хост_Отверстия_{doc.Application.Username}",
                new BuiltInCategory[] { BuiltInCategory.OST_Floors },
                new FilterRule[] { }
                );
            using(Transaction t = doc.StartTransaction("Обновление фильтра перекрытий")) {
                // переназначить категории элементов, если пользователь изменил их
                floorsFilter.SetCategories(new ElementId[] { new ElementId(BuiltInCategory.OST_Floors) });
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
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static ElementFilter GetHighlightElementFilter(Wall wallToHighlight) {
            if(wallToHighlight is null) { throw new ArgumentNullException(nameof(wallToHighlight)); }

            // имя типа стены
            BuiltInParameter typeNameParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
            string wallTypeName = wallToHighlight.WallType.Name;

            // отсеиваем все стены, у которых название типа не равно заданному
            FilterRule typeNameNotEqualsRule = new NotEqualsVisister().Create(new ElementId(typeNameParam), wallTypeName);
            ElementParameterFilter typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);


            // длина стены
            BuiltInParameter lengthParam = BuiltInParameter.CURVE_ELEM_LENGTH;
            double wallLength = wallToHighlight.GetParamValue<double>(lengthParam);
            ElementId lengthParamId = new ElementId(lengthParam);

            // отсеиваем все стены, длина которых меньше заданной
            FilterRule lengthLessRule = new LessVisister().Create(lengthParamId, wallLength);
            ElementParameterFilter lengthLessFilter = new ElementParameterFilter(lengthLessRule);

            // отсеиваем все стены, длина которых больше заданной
            FilterRule lengthGreaterRule = new GreaterVisister().Create(lengthParamId, wallLength);
            ElementParameterFilter lengthGreaterFilter = new ElementParameterFilter(lengthGreaterRule);

            return new LogicalOrFilter(new List<ElementFilter>() {
                typeNameNotEqualsFilter,
                lengthLessFilter,
                lengthGreaterFilter
            });
        }

        /// <summary>
        /// Создает фильтр, в который попадают все перекрытия, кроме заданного
        /// </summary>
        /// <param name="floorToHighlight">Перекрытие, которое не должно проходить фильтр</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static ElementFilter GetHighlightElementFilter(Floor floorToHighlight) {
            if(floorToHighlight is null) { throw new ArgumentNullException(nameof(floorToHighlight)); }

            // имя типа перекрытия
            BuiltInParameter typeNameParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
            string floorTypeName = floorToHighlight.FloorType.Name;

            // отсеиваем все перекрытия, у которых название типа не равно заданному
            FilterRule typeNameNotEqualsRule = new NotEqualsVisister().Create(new ElementId(typeNameParam), floorTypeName);
            ElementParameterFilter typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);


            // периметр перекрытия
            BuiltInParameter perimeterParam = BuiltInParameter.HOST_PERIMETER_COMPUTED;
            double floorPerimeter = floorToHighlight.GetParamValue<double>(perimeterParam);
            ElementId perimeterParamId = new ElementId(perimeterParam);

            // отсеиваем все перекрытия, периметр которых меньше заданного
            FilterRule perimeterLessRule = new LessVisister().Create(perimeterParamId, floorPerimeter);
            ElementParameterFilter perimeterLessFilter = new ElementParameterFilter(perimeterLessRule);

            // отсеиваем все перекрытия, периметр которых больше заданного
            FilterRule perimeterGreaterRule = new GreaterVisister().Create(perimeterParamId, floorPerimeter);
            ElementParameterFilter perimeterGreaterFilter = new ElementParameterFilter(perimeterGreaterRule);

            return new LogicalOrFilter(new List<ElementFilter> {
                typeNameNotEqualsFilter,
                perimeterLessFilter,
                perimeterGreaterFilter
            });
        }
    }
}
