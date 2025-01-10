using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;


namespace RevitRoomExtrusion.Models {
    internal class RoomElement {
        private readonly RevitRepository _revitRepository;
        private readonly Document _document;
        private readonly Room _room;
        private readonly View3D _view3D;

        public RoomElement(RevitRepository revitRepository, Room room, View3D view3D) {
            _revitRepository = revitRepository;
            _document = _revitRepository.Document;
            _room = room;
            _view3D = view3D;

            LocationSlab = CalculateLocation();
            LocationPoint locationPoint = room.Location as LocationPoint;
            LocationRoom = locationPoint.Point.Z;
            ArrArray = GetArrArray();
        }

        public double LocationSlab { get; private set; }
        public double LocationRoom { get; private set; }
        public CurveArrArray ArrArray { get; private set; }

        private double CalculateLocation() {
            
            BoundingBoxXYZ boundingBox = _room.get_BoundingBox(null);
            XYZ minBB = boundingBox.Min;
            XYZ maxBB = boundingBox.Max;
            
            XYZ pointCenter = (maxBB + minBB) / 2;
            XYZ pointDirection = new XYZ(pointCenter.X, pointCenter.Y, -100000);

            List<BuiltInCategory> collectionFilter = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_Floors
            };
            
            ElementMulticategoryFilter multiCategoryFilter = new ElementMulticategoryFilter(collectionFilter);            
            ReferenceIntersector refIntersec = new ReferenceIntersector(multiCategoryFilter, FindReferenceTarget.Element, _view3D);            
            
            ReferenceWithContext referenceWithContext = refIntersec.FindNearest(pointCenter, pointDirection);
            ElementId foundElementId = referenceWithContext.GetReference().ElementId;

            Element foundElement = _document.GetElement(foundElementId);
            double foundElementLocation = foundElement.get_BoundingBox(null).Max.Z;

            return foundElementLocation;
        }
        private CurveArrArray GetArrArray() {
            IList<IList<BoundarySegment>> boundarySegments = _room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            CurveArray curveArray = new CurveArray();
            CurveArrArray curveArrArray = new CurveArrArray();

            foreach(BoundarySegment boundarySegment in boundarySegments.FirstOrDefault()) {
                curveArray.Append(boundarySegment.GetCurve());
            }
            curveArrArray.Append(curveArray);
            return curveArrArray;
        }
    }
}
