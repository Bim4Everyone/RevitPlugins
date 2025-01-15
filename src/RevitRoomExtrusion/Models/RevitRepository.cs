using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;


namespace RevitRoomExtrusion.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;             
        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;         

        public View3D GetView3D(string name) {
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

            var existingView = views.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if(existingView != null) {
                return existingView as View3D;
            }
            View3D view3D = View3D.CreateIsometric(Document, viewType3D.Id);
            view3D.Name = name;
            return view3D;
        }
        public List<Room> GetRooms() {
            ICollection<ElementId> selectedObjectIds = ActiveUIDocument.Selection.GetElementIds();
            List<Room> rooms = new List<Room>();
            foreach(ElementId id in selectedObjectIds) {
                Element element = Document.GetElement(id);
                if(element is Room room) {
                    rooms.Add(room);
                }
            }
            return rooms;
        } 
        public void SetRoom(ElementId elementId) {
            List<ElementId> listRoomElements = new List<ElementId>();            
            listRoomElements.Add(elementId);            
            ActiveUIDocument.Selection.SetElementIds(listRoomElements);
        }
    }
}
