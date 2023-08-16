using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitOpeningPlacement.Models.Selection {
    /// <summary>
    /// Фильтр выбора элементов заданных типов из активного документа. Например, <see cref="Wall"/>
    /// </summary>
    internal class SelectionFilterElementsOfClasses : ISelectionFilter {
        private readonly ICollection<Type> _categories;


        /// <summary>
        /// Конструктор выбора элементов заданных типов из активного документа, например, Wall
        /// </summary>
        /// <param name="types">Типы для выбора</param>
        public SelectionFilterElementsOfClasses(ICollection<Type> types) {
            _categories = types;
        }


        public bool AllowElement(Element elem) {
            if(elem is null) { return false; }

            return _categories.Contains(elem.GetType());
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
