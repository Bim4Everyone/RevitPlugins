using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Selection {
    internal class SelectionFilterMepElements : ISelectionFilter {
        private readonly ICollection<BuiltInCategory> _categories;

        public SelectionFilterMepElements(ICollection<BuiltInCategory> categories) {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }


        public bool AllowElement(Element elem) {
            if(elem is null) { return false; }

            return _categories.Contains(elem.Category.GetBuiltInCategory());
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
