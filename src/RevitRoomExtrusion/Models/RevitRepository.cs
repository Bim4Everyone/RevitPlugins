using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models {
    internal class RevitRepository {
        private readonly ILocalizationService _localizationService;

        public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
            UIApplication = uiApplication;
            _localizationService = localizationService;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public View3D GetView3D(string familyName) {
            string name3Dview = String.Format(
                _localizationService.GetLocalizedString("RevitRepository.ViewName"), Application.Username, familyName);

            var views = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();
            var existingView = views
                .FirstOrDefault(v => v.Name.Equals(name3Dview, StringComparison.OrdinalIgnoreCase));

            if(existingView != null) {
                return existingView as View3D;
            } else {
                return CreateView3D(name3Dview);
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

        public FamilySymbol GetFamilySymbol(Family family) {
            ElementFilter filter = new FamilySymbolFilter(family.Id);
            return new FilteredElementCollector(Document)
                .WherePasses(filter)
                .Cast<FamilySymbol>()
                .First();
        }

        private View3D CreateView3D(string name3Dview) {
            var viewTypes = new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .ToElements()
                .Cast<ViewFamilyType>();
            var viewTypes3D = viewTypes
                .Where(vt => vt.ViewFamily == ViewFamily.ThreeDimensional)
                .First();
            string transactionName = _localizationService.GetLocalizedString("RevitRepository.TransactionNameCreate");
            using(Transaction t = Document.StartTransaction(transactionName)) {
                View3D view3D = View3D.CreateIsometric(Document, viewTypes3D.Id);
                view3D.Name = name3Dview;
                t.Commit();
                return view3D;
            }
        }
    }
}
