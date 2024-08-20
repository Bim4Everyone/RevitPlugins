using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

namespace RevitRoomTagPlacement.Models {
    internal class TagPointFinder {
        private readonly Room _room;
        private readonly BoundingBoxXYZ _roomBB;
        private readonly double _indent;

        public TagPointFinder(Room room, double indent) {
            _room = room;
            _roomBB = room.GetBoundingBox();
            _indent = indent;
        }

        public UV GetPointByPlacementWay(PositionPlacementWay positionPlacementWay, View roomView) {
            double xValue;
            double yValue;
            double indentByScale = roomView.Scale * _indent;

            switch(positionPlacementWay) {
                case PositionPlacementWay.LeftTop:
                xValue = _roomBB.Min.X + indentByScale;
                yValue = _roomBB.Max.Y - indentByScale;
                break;
                case PositionPlacementWay.CenterTop:
                xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
                yValue = _roomBB.Max.Y - indentByScale;
                break;
                case PositionPlacementWay.RightTop:
                xValue = _roomBB.Max.X - indentByScale;
                yValue = _roomBB.Max.Y - indentByScale;
                break;
                case PositionPlacementWay.LeftCenter:
                xValue = _roomBB.Min.X + indentByScale;
                yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;
                break;
                case PositionPlacementWay.CenterCenter:
                xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
                yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;
                break;
                case PositionPlacementWay.RightCenter:
                xValue = _roomBB.Max.X - indentByScale;
                yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;
                break;
                case PositionPlacementWay.LeftBottom:
                xValue = _roomBB.Min.X + indentByScale;
                yValue = _roomBB.Min.Y + indentByScale;
                break;
                case PositionPlacementWay.CenterBottom:
                xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
                yValue = _roomBB.Min.Y + indentByScale;
                break;
                case PositionPlacementWay.RightBottom:
                xValue = _roomBB.Max.X - indentByScale;
                yValue = _roomBB.Min.Y + indentByScale;
                break;
                default:
                xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
                yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;
                break;
            }

            return new UV(xValue, yValue);
        }

        public UV GetPointByPath() {
            TriangulatedRoom triangulatedRoom = new TriangulatedRoom(_room);
            RoomPath path = new RoomPath(triangulatedRoom);

            return path.TagPoint;
        }
    }
}
