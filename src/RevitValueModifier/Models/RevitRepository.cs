using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitValueModifier.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Element> SelectedElements() {
            ICollection<ElementId> selectedIds = ActiveUIDocument.Selection.GetElementIds();
            var selectedElems = new List<Element>();
            if(selectedIds.Count == 0) {
                TaskDialog.Show("Ошибка!", "Не выбрано ни одного элемента");
                return selectedElems;
            }

            foreach(ElementId selectedId in selectedIds) {
                selectedElems.Add(Document.GetElement(selectedId));
            }
            return selectedElems;
        }
    }
}
