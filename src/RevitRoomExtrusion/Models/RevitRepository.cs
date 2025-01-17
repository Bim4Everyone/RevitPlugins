using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;


namespace RevitRoomExtrusion.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;             
        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public View3D GetView3D(string familyName) {
            var views = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();
            var viewTypes = new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .ToElements()
                .Cast<ViewFamilyType>();
            var viewTypes3D = viewTypes
                .Where(vt => vt.ViewFamily == ViewFamily.ThreeDimensional)
                .ToList();
            if(!viewTypes3D.Any()) {
                throw new InvalidOperationException("Тип 3D вид не найден!");
            }
            var viewType3D = viewTypes3D.First();

            string userName = Application.Username;
            string name3Dview = $"${userName}/{familyName}/Проверка дорожек";
            var existingView = views.FirstOrDefault(v => v.Name.Equals(name3Dview, StringComparison.OrdinalIgnoreCase));
            if(existingView != null) {
                return existingView as View3D;
            }

            using(Transaction t = Document.StartTransaction("BIM: Создание 3D вида проверки дорожек")) {                
                View3D view3D = View3D.CreateIsometric(Document, viewType3D.Id);
                view3D.Name = name3Dview;                
                t.Commit();
                return view3D;
            }            
        }

        public List<Room> GetSelectedRooms() {
            return ActiveUIDocument.Selection
                .GetElementIds()
                .Select(x => Document.GetElement(x))
                .OfType<Room>()
                .ToList();
        }

        public void SetSelectedRoom(ElementId elementId) {
            List<ElementId> listRoomElements = new List<ElementId>() { 
                elementId };                        
            ActiveUIDocument.Selection.SetElementIds(listRoomElements);
        }
    }
}

}
