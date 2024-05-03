using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Selection {
    /// <summary>
    /// Фильтр выбора пользователем экземпляров семейств заданий на отверстия из связанных файлов, подгруженных в активный документ
    /// </summary>
    internal class SelectionFilterOpeningMepTasksIncoming : ISelectionFilter {
        private readonly Document _activeDocument;

        /// <summary>
        /// Конструктор фильтра выбора пользователем экземпляров семейств заданий на отверстия из связанных файлов, подгруженных в активный документ
        /// </summary>
        /// <param name="activeDocument">Активный документ, в который подгружены связи с заданиями на отверстия</param>
        public SelectionFilterOpeningMepTasksIncoming(Document activeDocument) {
            _activeDocument = activeDocument ?? throw new System.ArgumentNullException(nameof(activeDocument));
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
                    && (famInst.Category.GetBuiltInCategory() == BuiltInCategory.OST_GenericModel)
                    && RevitRepository.OpeningTaskTypeName.Any(n => n.Value.Equals(famInst.Name))
                    && RevitRepository.OpeningTaskFamilyName.Any(n => n.Value.Equals(famInst.Symbol?.FamilyName));
            }
            return false;
        }
    }
}
