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

        /// <summary>
        /// Возвращает фильтр по параметрам элементов на виде, выключив видимость элементов которого, 
        /// скроются все элементы той же категории, что и заданный элемент, кроме заданного элемента
        /// </summary>
        /// <param name="document">Документ, в котором нужно получить фильтр</param>
        /// <param name="element">Элемент, который не должен остаться видимым</param>
        /// <param name="filterName">Название фильтра по параметрам</param>
        /// <returns>Существующий обновленный или снова созданный и обновленный фильтр по параметрам</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ParameterFilterElement GetHighlightFilter(Document document, Element element, string filterName) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }
            if(string.IsNullOrWhiteSpace(filterName)) { throw new ArgumentException(nameof(filterName)); }

            var elementFilter = GetOrCreateFilter(document, filterName, element.Category.GetBuiltInCategory());
            return UpdateHighlightFilter(elementFilter, element);
        }

        /// <summary>
        /// Возвращает фильтр по параметрам элементов на виде, выключив видимость элементов которого,
        /// скроются все элементы той же категории, что и заданные элементы, кроме самих заданных элементов.
        /// При этом заданные элементы должны быть строго одной и той же категории
        /// </summary>
        /// <param name="document"></param>
        /// <param name="firstEl">Первый элемент</param>
        /// <param name="secondEl">Второй элемент с такой же категорией как у первого</param>
        /// <param name="filterName">Название фильтра</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public ParameterFilterElement GetHighlightFilter(
            Document document,
            Element firstEl,
            Element secondEl,
            string filterName) {

            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(firstEl is null) { throw new ArgumentNullException(nameof(firstEl)); }
            if(secondEl is null) { throw new ArgumentNullException(nameof(secondEl)); }
            if(string.IsNullOrWhiteSpace(filterName)) { throw new ArgumentException(nameof(filterName)); }
            if(firstEl.Category.GetBuiltInCategory() != secondEl.Category.GetBuiltInCategory()) {
                throw new InvalidOperationException($"У элементов разные категории");
            }

            var filter = GetOrCreateFilter(document, filterName, firstEl.Category.GetBuiltInCategory());
            return UpdateHighlightFilter(filter, firstEl, secondEl);
        }

        /// <summary>
        /// Возвращает фильтр с заданным названием по всем категориям элементов модели, кроме заданных категорий
        /// </summary>
        /// <param name="document">Документ, в котором должен быть получен фильтр</param>
        /// <param name="exceptCategories">Категории, которых не должно быть в фильтре</param>
        /// <param name="filterName">Название фильтра</param>
        /// <returns>Существующий фильтр с переназначенными категориями или созданный фильтр</returns>
        public ParameterFilterElement GetExceptCategoriesFilter(
            Document document,
            ICollection<BuiltInCategory> exceptCategories,
            string filterName) {

            var categoriesToFilter = GetAllModelCategories(document);
            categoriesToFilter.ExceptWith(exceptCategories);
            var filter = GetOrCreateFilter(document, filterName, categoriesToFilter);
            return UpdateHighlightFilter(filter, categoriesToFilter);
        }

        /// <summary>
        /// Возвращает все категории модели из документа
        /// </summary>
        /// <param name="document">Документ с категориями</param>
        /// <returns></returns>
        private static HashSet<BuiltInCategory> GetAllModelCategories(Document document) {
            Categories allCategories = document.Settings.Categories;
            HashSet<BuiltInCategory> modelCategories = new HashSet<BuiltInCategory>();
            foreach(Category category in allCategories) {
                if(category.CategoryType == CategoryType.Model && category.IsVisibleInUI) {
                    modelCategories.Add(category.GetBuiltInCategory());
                }
            }
            return modelCategories;
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

            var categoriesIds = categories
                .Select(item => new ElementId(item))
                .ToArray();

            ParameterFilterElement filter = new FilteredElementCollector(document)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .FirstOrDefault(item => item.Name.Equals(filterName));
            if(filter == null) {
                filter = ParameterFilterElement.Create(document, filterName, categoriesIds);
            }

            return filter;
        }

        /// <summary>
        /// Переназначает категории для фильтра и сбрасывает критерии фильтрации
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="categoriesToFilter"></param>
        /// <returns></returns>
        private ParameterFilterElement UpdateHighlightFilter(
            ParameterFilterElement filter,
            ICollection<BuiltInCategory> categoriesToFilter) {

            //переназначить категории элементов, если пользователь изменил их
            filter.SetCategories(categoriesToFilter.Select(c => new ElementId(c)).ToArray());
            //сбросить все критерии фильтрации
            filter.ClearRules();
            return filter;
        }

        /// <summary>
        /// Обновляет фильтр по параметрам элементов на виде, который нужен для скрытия элементов той же категории, 
        /// что и заданный элемент, но отличных от него
        /// </summary>
        /// <param name="filter">Фильтр по параметрам элементов на виде, который надо обновить</param>
        /// <param name="element">Элемент, который выделяется фильтром. То есть элемент, который не попадает в фильтр.</param>
        /// <returns></returns>
        private ParameterFilterElement UpdateHighlightFilter(
            ParameterFilterElement filter,
            Element element) {

            //переназначить категории элементов, если пользователь изменил их
            filter.SetCategories(new ElementId[] { new ElementId(element.Category.GetBuiltInCategory()) });
            //переназначить критерии фильтрации
            filter.SetElementFilter(GetHighlightElementFilter(element, filter));
            return filter;
        }

        /// <summary>
        /// Обновляет фильтр по параметрам элементов на виде, который нужен для скрытия элементов той же категории, 
        /// что и заданные элементы, но отличные от них.
        /// При этом заданные элементы должны быть строго одной и той же категории
        /// </summary>
        /// <param name="filter">Фильтр по параметрам элементов на виде, который надо обновить</param>
        /// <param name="firstEl">Первый элемент</param>
        /// <param name="secondEl">Второй элемент</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private ParameterFilterElement UpdateHighlightFilter(
            ParameterFilterElement filter,
            Element firstEl,
            Element secondEl) {
            if(firstEl.Category.Id != secondEl.Category.Id) {
                throw new InvalidOperationException("У элементов разные категории");
            }

            //переназначить категории элементов, если пользователь изменил их
            filter.SetCategories(new ElementId[] { firstEl.Category.Id });
            //переназначить критерии фильтрации
            filter.SetElementFilter(GetHighlightElementFilter(firstEl, secondEl, filter));
            return filter;
        }

        /// <summary>
        /// Возвращает фильтр, в который попадают все элементы категории заданного элемента, 
        /// значение типоразмера и значение double параметров которых отличаются 
        /// от соответствующих значений параметров заданного элемента
        /// </summary>
        /// <param name="element"></param>
        /// <param name="checker">Экземпляр класса для проверки возможности установить фильтры на параметры</param>
        /// <returns>Фильтр вида: Имя типа != "имя типа" ИЛИ Размер != ХХХ ИЛИ Высота != ХХХ и т.д.</returns>
        private ElementFilter GetHighlightElementFilter(Element element, ParameterFilterElement checker) {
            List<ElementFilter> filters = new List<ElementFilter>();
            //проверяем наличие типа у элемента
            if(element.HasElementType()) {
                BuiltInParameter typeName = BuiltInParameter.ALL_MODEL_TYPE_NAME;
                string elType = element.GetElementType().Name;
                //отсеиваем все элементы, у которых название типа не равно названию типа заданного элемента
                FilterRule typeNameNotEqualsRule = _notEqualsVisister.Create(new ElementId(typeName), elType);
                var typeNameNotEqualsFilter = new ElementParameterFilter(typeNameNotEqualsRule);
                if(IsValidFilter(checker, typeNameNotEqualsFilter)) {
                    filters.Add(typeNameNotEqualsFilter);
                }
            }

            IEnumerable<Parameter> doubleParameters = GetParameters(element)
                .Where(p => p.HasValue && p.StorageType == StorageType.Double && p.AsDouble() != 0);
            foreach(var param in doubleParameters) {
                FilterRule rule = _notEqualsVisister.Create(param.Id, param.AsDouble());
                var paramFilter = new ElementParameterFilter(rule);
                if(IsValidFilter(checker, paramFilter)) {
                    filters.Add(paramFilter);
                }
            }
            return new LogicalOrFilter(filters);
        }

        /// <summary>
        /// Возвращает фильтр, в который попадают все элементы категории заданных элементов,
        /// значения типоразмеров и значения double параметров которых отличаются 
        /// от соответствующих значений параметров заданных элементов.
        /// При этом заданные элементы должны быть строго одной и той же категории.
        /// </summary>
        /// <param name="firstEl">Первый элемент</param>
        /// <param name="secondEl">Второй элемент с такой же категорией как у первого</param>
        /// <param name="checker">Экземпляр класса для проверки возможности установить фильтры на параметры</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private ElementFilter GetHighlightElementFilter(
            Element firstEl,
            Element secondEl,
            ParameterFilterElement checker) {
            if(firstEl.Category.Id != secondEl.Category.Id) {
                throw new InvalidOperationException("У элементов разные категории");
            }

            var firstOrFilter = GetHighlightElementFilter(firstEl, checker);
            var secondOrFilter = GetHighlightElementFilter(secondEl, checker);

            return new LogicalAndFilter(firstOrFilter, secondOrFilter);
        }

        /// <summary>
        /// Проверяет, что заданный фильтр может быть использован 
        /// внутри фильтра по параметрам для фильтрации элементов на виде
        /// </summary>
        /// <param name="checker">Фильтр по параметрам на виде</param>
        /// <param name="elementFilter">Фильтр, который надо проверить</param>
        /// <returns></returns>
        private bool IsValidFilter(ParameterFilterElement checker, ElementFilter elementFilter) {
            return checker.ElementFilterIsAcceptableForParameterFilterElement(elementFilter)
                && checker.AllRuleParametersApplicable(elementFilter);
        }

        private IEnumerable<Parameter> GetParameters(Element element) {
            return element.Parameters.OfType<Parameter>();
        }
    }
}
