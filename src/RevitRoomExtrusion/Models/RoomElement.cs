using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;


namespace RevitRoomExtrusion.Models {
    internal class RoomElement {
        private readonly RevitRepository _revitRepository;
        private readonly Room _room;
        private readonly View3D _view3D;
        private readonly double _normalDirection = -100000;

        private static readonly ElementMulticategoryFilter _multiCategoryFilter = new ElementMulticategoryFilter(
            new BuiltInCategory[] {
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_Floors
            });

        public RoomElement(RevitRepository revitRepository, Room room, View3D view3D) {
            _revitRepository = revitRepository;
            _room = room;
            _view3D = view3D;
            LocationPoint locationPoint = room.Location as LocationPoint;
            LocationRoom = locationPoint.Point.Z;
            LocationSlab = CalculateLocation();
            ArrArray = GetArrArray();
        }

        public double LocationRoom { get; private set; }
        public int LocationSlab { get; private set; }
        public CurveArrArray ArrArray { get; private set; }

        private int CalculateLocation() {
            BoundingBoxXYZ boundingBox = _room.get_BoundingBox(null);
            XYZ pointCenter = (boundingBox.Max + boundingBox.Min) / 2;
            XYZ pointDirection = new XYZ(pointCenter.X, pointCenter.Y, _normalDirection);

            ReferenceWithContext referenceWithContext = GetReferenceWithContext(pointCenter, pointDirection);
            double foundElementLocation;
            if(referenceWithContext != null) {
                double proximity = referenceWithContext.Proximity;
                foundElementLocation = pointCenter.Z - proximity - _revitRepository.GetBasePointLocation();
            } else {
                foundElementLocation = boundingBox.Min.Z;
            }
            double convertedFoundElementLocation = UnitUtils.ConvertFromInternalUnits(
                foundElementLocation, UnitTypeId.Millimeters);
            return Convert.ToInt32(Math.Round(convertedFoundElementLocation));
        }

        private CurveArrArray GetArrArray() {
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> listListSegments = _room.GetBoundarySegments(options);

            CurveArrArray curveArrArray = new CurveArrArray();
            foreach(IList<BoundarySegment> listSegments in listListSegments) {
                CurveArray curveArray = new CurveArray();
                foreach(BoundarySegment boundarySegment in listSegments) {
                    curveArray.Append(boundarySegment.GetCurve());
                }
                curveArrArray.Append(curveArray);
            }
            return curveArrArray;
        }

        private ReferenceWithContext GetReferenceWithContext(XYZ pointCenter, XYZ pointDirection) {
            ReferenceIntersector refIntersec = new ReferenceIntersector(
                _multiCategoryFilter, FindReferenceTarget.Element, _view3D);
            return refIntersec.FindNearest(pointCenter, pointDirection);
        }
    }
}
