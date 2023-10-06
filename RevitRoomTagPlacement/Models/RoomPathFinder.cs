﻿using System;
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
        private const double Indent = 0.0229659;
        private Room _room;
        private BoundingBoxXYZ _roomBB;

        public RoomPathFinder(Room room) {
            _room = room;
            _roomBB = room.GetBoundingBox();
        }

        public UV GetPointByPlacementWay(PositionPlacementWay positionPlacementWay, View roomView) {

            double xValue;
            double yValue;
            double indentByScale = roomView.Scale * Indent;

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
                xValue = _roomBB.Max.X - roomView.Scale * Indent;
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
            var xValue = (_roomBB.Min.X + _roomBB.Max.X) * 0.5;
            var yValue = (_roomBB.Min.Y + _roomBB.Max.Y) * 0.5;

            return new UV(xValue, yValue);
        }
    }
}
