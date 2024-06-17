using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitOpeningPlacement.Models.Selection {
    internal class SelectionFilterMepElements : ISelectionFilter {
        private readonly RevitRepository _revitRepository;
        private readonly ICollection<MepCategoryEnum> _categories;

        public SelectionFilterMepElements(RevitRepository revitRepository, ICollection<MepCategoryEnum> categories) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }


        public bool AllowElement(Element elem) {
            if(elem is null || elem.Category is null) { return false; }

            return _categories.Any(c => _revitRepository.ElementBelongsToMepCategory(c, elem));
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
