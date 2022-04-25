using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal abstract class BaseWindow : ICanPlaceWindowGap {
        private readonly Element _element;
        protected readonly RevitRepository _revitRepository;

        public BaseWindow(Element element, RevitRepository revitRepository) {
            _element = element;
            _revitRepository = revitRepository;
        }


        protected abstract XYZ GetPlaceLocation();
        protected abstract IEnumerable<HostObject> GetHostElements();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);

        public List<FamilyInstance> PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            return GetPlacedFamilyInstances(document, windowGapType).ToList();
        }

        private IEnumerable<FamilyInstance> GetPlacedFamilyInstances(Document document, FamilySymbol windowGapType) {
            XYZ location = GetPlaceLocation();
            IEnumerable<HostObject> hostElements = GetHostElements().Distinct(new Sho());

            foreach(HostObject hostObject in hostElements) {
                Reference face = HostObjectUtils
                    .GetSideFaces(hostObject, ShellLayerType.Exterior)
                    .FirstOrDefault();

                FamilyInstance windowGap =
                    document.Create.NewFamilyInstance(face, location, XYZ.Zero, windowGapType);

                windowGap.SetParamValue("Окно ID", _element.Id.IntegerValue);
                yield return UpdateParamsWindowGap(windowGap);
            }
        }
    }
}