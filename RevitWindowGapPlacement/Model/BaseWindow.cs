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
        protected abstract Wall GetHostElement();
        protected abstract FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap);

        public FamilyInstance PlaceWindowGap(Document document, FamilySymbol windowGapType) {
            XYZ location = GetPlaceLocation();
            Wall hostElement = GetHostElement();

            Reference face = HostObjectUtils
                .GetSideFaces(hostElement, ShellLayerType.Exterior)
                .FirstOrDefault();

            FamilyInstance windowGap =
                document.Create.NewFamilyInstance(face, location, XYZ.Zero, windowGapType);

            return UpdateParamsWindowGap(windowGap);
        }
    }
}