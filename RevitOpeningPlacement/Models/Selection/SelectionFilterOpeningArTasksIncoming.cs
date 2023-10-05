using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Selection {
    /// <summary>
    /// Фильтр выбора пользователем экземпляров семейств заданий на отверстия от АР из связанных файлов АР, подгруженных в активный документ
    /// </summary>
    internal class SelectionFilterOpeningArTasksIncoming : ISelectionFilter {
        private readonly Document _activeDocument;


        /// <summary>
        /// Конструктор фильтра выбора пользователем экземпляров семейств заданий на отверстия от АР из связанных файлов АР, подгруженных в активный документ
        /// </summary>
        /// <param name="activeDocument">Активный документ КР, в который подгружены связи АР с заданиями на отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SelectionFilterOpeningArTasksIncoming(Document activeDocument) {
            _activeDocument = activeDocument ?? throw new ArgumentNullException(nameof(activeDocument));
        }


        public bool AllowElement(Element elem) {
            return elem != null;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            Element revitLink = _activeDocument.GetElement(reference);

            if((revitLink != null) && (revitLink is RevitLinkInstance link)) {
                var linkDocument = link.GetLinkDocument();
                var element = linkDocument.GetElement(reference.LinkedElementId);

                return (element != null)
                    && (element is FamilyInstance famInst)
                    && (famInst.Category.GetBuiltInCategory() == BuiltInCategory.OST_Windows)
                    && RevitRepository.OpeningRealArTypeName.Any(n => n.Value.Equals(famInst.Name))
                    && RevitRepository.OpeningRealArFamilyName.Any(n => n.Value.Equals(famInst.Symbol?.Name));
            }
            return false;
        }
    }
}
