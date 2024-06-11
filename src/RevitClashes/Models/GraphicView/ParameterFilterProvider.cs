using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Visiter;

namespace RevitClashDetective.Models.GraphicView {
    internal class ParameterFilterProvider {
        private readonly NotEqualsVisister _notEqualsVisister;

        public ParameterFilterProvider() {
            _notEqualsVisister = new NotEqualsVisister();
        }


        public ParameterFilterElement GetHighlightFilter(Document document, Element element, string filterName) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }
            if(string.IsNullOrWhiteSpace(filterName)) { throw new ArgumentException(nameof(filterName)); }

            var elementFilter = GetOrCreateFilter(document, filterName, element.Category.GetBuiltInCategory());
            return UpdateHighlightFilter(document, elementFilter, element);
        }


        private ParameterFilterElement GetOrCreateFilter(
            Document document,
            string filterName,
            BuiltInCategory category) {

            return GetOrCreateFilter(document, filterName, new BuiltInCategory[] { category });
        }

        /// <summary>
        /// Возвращает существующий или создает новый фильтр для настроек графики на виде
        /// </summary>
        /// <param name="document">Документ, в котором нужно получить фильтр</param>
        /// <param name="filterName">Название фильтра</param>
        /// <param name="categories">Категории элементов для фильтра</param>
        /// <returns></returns>
        private ParameterFilterElement GetOrCreateFilter(
            Document document,
            string filterName,
            ICollection<BuiltInCategory> categories) {

            ParameterFilterElement filter = new FilteredElementCollector(document)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .FirstOrDefault(item => item.Name.Equals(filterName));
            if(filter == null) {
                using(Transaction t = document.StartTransaction("Создание фильтра")) {
                    filter = ParameterFilterElement.Create(
                        document,
                        filterName,
                        categories
                            .Select(item => new ElementId(item))
                            .ToArray());

                    t.Commit();
                }
            }

            return filter;
        }

        private ParameterFilterElement UpdateHighlightFilter(
            Document document,
            ParameterFilterElement filter,
            Element element) {

            using(Transaction t = document.StartTransaction("Обновление фильтра")) {
                //переназначить категории элементов, если пользователь изменил их
                filter.SetCategories(new ElementId[] { new ElementId(element.Category.GetBuiltInCategory()) });
                //сбросить все критерии фильтрации
                filter.ClearRules();
                //установить критерии фильтрации
                filter.SetElementFilter(GetGighlightElementFilter(element));
                t.Commit();
            }
            return filter;
        }

        /// <summary>
        /// Возвращает фильтр, в который попадают все элементы, 
        /// значение типоразмера и значение double параметров которых отличаются 
        /// от соответствующих значений параметров заданного элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Фильтр вида: Имя типа != "имя типа" ИЛИ Размер != ХХХ ИЛИ Высота != ХХХ и т.д.</returns>
        private ElementFilter GetGighlightElementFilter(Element element) {
            //имя типа элемента
            BuiltInParameter typeName = BuiltInParameter.ALL_MODEL_TYPE_NAME;
            string elType = element.GetParamValue<string>(typeName);

            //отсеиваем все элементы, у которых название типа не равно названию типа заданного элемента
            FilterRule typeNameNotEqualsRule = _notEqualsVisister.Create(new ElementId(typeName), elType);
            ElementParameterFilter typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);
            List<ElementFilter> filters = new List<ElementFilter> {
                typeNameNotEqualsFilter
            };

            var doubleParameters = GetParameters(element).Where(p => p.HasValue && p.StorageType == StorageType.Double);
            foreach(var param in doubleParameters) {
                FilterRule rule = _notEqualsVisister.Create(param.Id, param.AsDouble());
                ElementParameterFilter paramFilter = new ElementParameterFilter(rule);
                filters.Add(paramFilter);
            }
            return new LogicalOrFilter(filters);
        }

        private ICollection<Parameter> GetParameters(Element element) {
            List<Parameter> parameters = new List<Parameter>();
            foreach(Parameter parameter in element.Parameters) {
                parameters.Add(parameter);
            }
            return parameters;
        }
    }
}
