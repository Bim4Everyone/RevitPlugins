using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseWindow : ICanPlaceWindowGap {
        private readonly Element _element;
        protected readonly RevitRepository _revitRepository;

        public BaseWindow(Element element, RevitRepository revitRepository) {
            _element = element;
            _revitRepository = revitRepository;
        }


        protected abstract XYZ GetPlaceLocation();
        protected abstract IEnumerable<Element> GetHostElements();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);

        public List<FamilyInstance> PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            return GetPlacedFamilyInstances(document, windowGapType).ToList();
        }

        private IEnumerable<FamilyInstance> GetPlacedFamilyInstances(Document document, FamilySymbol windowGapType) {
            XYZ location = GetPlaceLocation();
            IEnumerable<Element> hostElements = GetHostElements().Distinct(new Sho());

            foreach(Element element in hostElements) {
                if(element is HostObject hostObject) {
                    Reference face = HostObjectUtils
                        .GetSideFaces(hostObject, ShellLayerType.Exterior)
                        .FirstOrDefault();

                    FamilyInstance windowGap =
                        document.Create.NewFamilyInstance(face, location, XYZ.Zero, windowGapType);

                    yield return UpdateParamsWindowGap(windowGap);
                }

                if(element is FamilyInstance familyInstance) {
                    var geometryInstance = familyInstance
                        .get_Geometry(new Options() {View = document.ActiveView, ComputeReferences = true})
                        .OfType<GeometryInstance>()
                        .FirstOrDefault();

                    if(geometryInstance != null) {
                        var solid = geometryInstance.GetInstanceGeometry()
                            .OfType<Solid>()
                            .OrderByDescending(item => item.Volume)
                            .FirstOrDefault();
                        
                        if(solid != null) {
                            var face = solid.Faces
                                .OfType<RuledFace>()
                                .OrderByDescending(item => item.Area)
                                .FirstOrDefault();

                            if(face != null) {
                                FamilyInstance windowGap =
                                    document.Create.NewFamilyInstance(face, location, XYZ.Zero, windowGapType);

                                yield return UpdateParamsWindowGap(windowGap);
                            }
                        }
                    }
                }
            }
        }
    }
}