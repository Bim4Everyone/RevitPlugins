using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews {
    /// <summary>
    /// Класс, предоставляющий настройки видимости категорий модели на 3D виде
    /// </summary>
    internal class VisibleModelCategoriesSetting : IView3DSetting {
        private readonly ICollection<BuiltInCategory> _categories;


        /// <summary>
        /// Конструктор класса, предоставляющего настройки видимости категорий модели на 3D виде
        /// <para>Все категории модели, кроме заданных, будут скрыты</para>
        /// </summary>
        /// <param name="document">Документ, в котором находится 3D вид</param>
        /// <param name="modelCategories">Категории модели, которые нужно оставить видимыми на виде</param>
        /// <exception cref="ArgumentNullException"></exception>
        public VisibleModelCategoriesSetting(ICollection<BuiltInCategory> modelCategories) {
            _categories = modelCategories ?? throw new ArgumentNullException(nameof(modelCategories));
        }


        public void Apply(View3D view3D) {
            var categoriesToHide = GetModelCategoriesToHide(view3D);

            foreach(ElementId category in categoriesToHide) {
                if(view3D.CanCategoryBeHidden(category)) {
                    view3D.SetCategoryHidden(category, true);
                }
            }
        }

        /// <summary>
        /// Возвращает коллекцию Id категорий модели, которые надо скрыть на 3D виде
        /// </summary>
        /// <param name="view3D">3D вид, на котором надо скрыть категории модели</param>
        /// <returns></returns>
        private ICollection<ElementId> GetModelCategoriesToHide(View3D view3D) {
            var document = view3D.Document;
            HashSet<ElementId> visibleCategories = GetVisibleModelCategories(document, _categories);
            HashSet<ElementId> allModelCategories = GetAllModelCategories(document);

            allModelCategories.ExceptWith(visibleCategories);

            return allModelCategories;
        }

        /// <summary>
        /// Возвращает все категории модели из документа
        /// </summary>
        /// <param name="document">Документ с категориями</param>
        /// <returns></returns>
        private HashSet<ElementId> GetAllModelCategories(Document document) {
            Categories allCategories = document.Settings.Categories;
            HashSet<ElementId> modelCategories = new HashSet<ElementId>();
            foreach(Category category in allCategories) {
                if(category.CategoryType == CategoryType.Model) {
                    modelCategories.Add(category.Id);
                }
            }
            return modelCategories;
        }

        /// <summary>
        /// Возвращает Id категорий модели, которые должны быть видны на 3D виде
        /// </summary>
        /// <param name="document">Документ с категориями</param>
        /// <param name="categories">Категории модели, которые должны быть видны</param>
        /// <returns></returns>
        private HashSet<ElementId> GetVisibleModelCategories(Document document, ICollection<BuiltInCategory> categories) {
            return categories
                 .Where(category => Category.GetCategory(document, category).CategoryType == CategoryType.Model)
                 .Select(category => new ElementId(category))
                 .ToHashSet();
        }
    }
}
