using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit3DvikSchemas.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public List<Element> GetHVACSystems() {
            List<Element> ductSystems = (List<Element>) new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_DuctSystem)
                .WhereElementIsNotElementType()
                .ToElements();

            List<Element> pipeSystems = (List<Element>) new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_PipingSystem)
                .WhereElementIsNotElementType()
                .ToElements();

            List<Element> allSystems = ductSystems.Concat(pipeSystems).ToList();

            return allSystems;

        }
    }
}
