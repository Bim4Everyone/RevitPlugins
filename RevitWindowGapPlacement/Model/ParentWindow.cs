using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal abstract class ParentWindow : BaseElement, ICanPlaceWindowGap {
        protected ParentWindow(Element element, RevitRepository revitRepository)
            : base(element, revitRepository) {
        }

        protected abstract IEnumerable<HostObject> GetHostElements();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);

        public List<FamilyInstance> PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            return GetPlacedFamilyInstances(document, windowGapType).ToList();
        }

        private IEnumerable<FamilyInstance> GetPlacedFamilyInstances(Document document, FamilySymbol windowGapType) {
            IEnumerable<HostObject> hostElements = GetHostElements().Distinct(new Sho());
            
            foreach(HostObject hostObject in hostElements) {
                Reference face = HostObjectUtils
                    .GetSideFaces(hostObject, ShellLayerType.Exterior)
                    .FirstOrDefault();

                FamilyInstance windowGap =
                    document.Create.NewFamilyInstance(face, PlaceLocation, XYZ.Zero, windowGapType);

                windowGap.SetParamValue("Окно ID", (int) _element.Id.GetIdValue());
                yield return UpdateParamsWindowGap(windowGap);
            }
        }
    }
}