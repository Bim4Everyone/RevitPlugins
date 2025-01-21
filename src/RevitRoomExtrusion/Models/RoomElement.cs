using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;


namespace RevitRoomExtrusion.Models {
    internal class RoomElement {
        
        private readonly Document _document;
        private readonly Room _room;
        private readonly View3D _view3D;
        private readonly double _normalDirection = -100000;

        public RoomElement(Document document, Room room, View3D view3D) {
            _document = document;
            _room = room;
            _view3D = view3D;            
            LocationPoint locationPoint = room.Location as LocationPoint;
            LocationRoom = locationPoint.Point.Z;
            LocationSlab = CalculateLocation();
            ArrArray = GetArrArray();
        }

        public double LocationRoom { get; private set; }
        public double LocationSlab { get; private set; }        
        public CurveArrArray ArrArray { get; private set; }

        private double CalculateLocation() {            
            BoundingBoxXYZ boundingBox = _room.get_BoundingBox(null);
            XYZ minBB = boundingBox.Min;
            XYZ maxBB = boundingBox.Max;            
            XYZ pointCenter = (maxBB + minBB) / 2;
            XYZ pointDirection = new XYZ(pointCenter.X, pointCenter.Y, _normalDirection);                        

            double proximity = GetReferenceWithContext(pointCenter, pointDirection).Proximity;
            double foundElementLocation = pointCenter.Z - proximity;
            double convertedFoundElementLocation =  UnitUtils.ConvertFromInternalUnits(foundElementLocation, 
                                                                                       UnitTypeId.Millimeters);            
            return Math.Round(convertedFoundElementLocation);
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
            List<BuiltInCategory> collectionFilter = new List<BuiltInCategory> {
                BuiltInCategory.OST_StructuralFoundation,
                BuiltInCategory.OST_Floors
            };
            ElementMulticategoryFilter multiCategoryFilter = new ElementMulticategoryFilter(collectionFilter);
            ReferenceIntersector refIntersec = new ReferenceIntersector(multiCategoryFilter,
                                                                        FindReferenceTarget.Element,
                                                                        _view3D);
            return refIntersec.FindNearest(pointCenter, pointDirection);            
        }
    }
}
