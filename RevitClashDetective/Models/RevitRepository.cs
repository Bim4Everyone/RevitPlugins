using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace RevitClashDetective.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public List<ParameterFilterElement> GetFilters() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .Where(item => item.Name.StartsWith("BIM"))
                .ToList();
        }

        private List<Solid> GetSolid(Element element) {
            List<Solid> solids = new List<Solid>();
            var option = new Options() { ComputeReferences = true };
            foreach(GeometryObject geometryObject in element.get_Geometry(option)) {
                if(geometryObject is Solid solid) {
                    solids.Add(solid);
                } else {
                    var geometryInstace = geometryObject as GeometryInstance;
                    if(geometryInstace == null)
                        continue;

                    foreach(var s in geometryInstace.GetInstanceGeometry().OfType<Solid>()) {
                        solids.Add(s);
                    }
                }
            }

            return UniteSolids(solids);
        }

        private List<Solid> UniteSolids(List<Solid> solids) {

            if(solids.Count == 0) {
                return null;
            }
            Solid union = solids[0];
            solids.RemoveAt(0);

            List<Solid> unitedSolids = new List<Solid>();

            foreach(var s in solids) {
                try {
                    union = BooleanOperationsUtils.ExecuteBooleanOperation(union, s, BooleanOperationsType.Union);
                } catch {
                    unitedSolids.Add(union);
                    union = s;
                }
            }

            return unitedSolids;
        }
    }
}
