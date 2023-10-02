using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

using pyRevitLabs.Json.Linq;

namespace RevitRoomTagPlacement.Models {
    internal class RoomPathFinder {
        private Room _room;
        private BoundingBoxXYZ _roomBB;

        public RoomPathFinder(Room room) {
            _room = room;
            _roomBB = room.GetBoundingBox();
        }

        public UV GetPointByPlacementWay(PositionPlacementWay positionPlacementWay) {
            
            double xValue;
            double yValue;

            switch(positionPlacementWay) {
                case PositionPlacementWay.LeftTop:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.1;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.9;
                break;
                case PositionPlacementWay.CenterTop:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.5;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.9;
                break;
                case PositionPlacementWay.RightTop:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.9;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.9;
                break;
                case PositionPlacementWay.LeftCenter:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.1;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.5;
                break;
                case PositionPlacementWay.CenterCenter:
                xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
                yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;
                break;
                case PositionPlacementWay.RightCenter:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.9;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.5;
                break;
                case PositionPlacementWay.LeftBottom:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.1;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.1;
                break;
                case PositionPlacementWay.CenterBottom:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.5;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.1;
                break;
                case PositionPlacementWay.RightBottom:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.9;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.1;
                break;
                default:
                xValue = _roomBB.Min.X + (_roomBB.Max.X - _roomBB.Min.X) * 0.5;
                yValue = _roomBB.Min.Y + (_roomBB.Max.Y - _roomBB.Min.Y) * 0.5;
                break;
            }

            return new UV(xValue, yValue);
        }

        public UV GetPointByPath() {
            var xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
            var yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;

            return new UV(xValue, yValue);
        }
    }
}
