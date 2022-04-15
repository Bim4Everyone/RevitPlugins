using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class BasicWindow : BaseWindow {
        private readonly FamilyInstance _familyInstance;

        public BasicWindow(FamilyInstance familyInstance, RevitRepository revitRepository)
            : base(familyInstance, revitRepository) {
            _familyInstance = familyInstance;
        }

        protected override IEnumerable<HostObject> GetHostElements() {
            XYZ point = ((LocationPoint) _familyInstance.Location).Point;
            return _revitRepository.GetNearestElements((HostObject) _familyInstance.Host, point,
                ((Wall) _familyInstance.Host).Orientation);
        }

        protected override XYZ GetPlaceLocation() {
            double height = _familyInstance.Symbol.GetParamValue<double>(BuiltInParameter.WINDOW_HEIGHT);
            XYZ point = new XYZ(0, 0, height);
            
            var wall = (Wall) _familyInstance.Host;
            if(wall.CrossSection == WallCrossSection.SingleSlanted) {
                var radian = wall.GetParamValue<double>(BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL);
                
                LocationCurve location = (LocationCurve) wall.Location;
                Line line = (Line) location.Curve;

                point = Transform.CreateRotation(line.Direction, radian).OfVector(point);
                return ((LocationPoint) _familyInstance.Location).Point.Add(point);
            }
            
            return ((LocationPoint) _familyInstance.Location).Point.Add(point);
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            double width = _familyInstance.Symbol.GetParamValue<double>(BuiltInParameter.WINDOW_WIDTH);
            double height = _familyInstance.Symbol.GetParamValue<double>(BuiltInParameter.WINDOW_HEIGHT);
            
            windowGap.SetParamValue(BuiltInParameter.WINDOW_WIDTH, width);
            windowGap.SetParamValue(BuiltInParameter.WINDOW_HEIGHT, height);
            
            windowGap.SetParamValue("Смещение Сверху",
                _familyInstance.Symbol.GetParamValue<double>("Четверть Сверху"));
            
            windowGap.SetParamValue("Смещение Справа", 
                _familyInstance.Symbol.GetParamValue<double>("Четверть Справа"));
            
            windowGap.SetParamValue("Смещение Слева",
                _familyInstance.Symbol.GetParamValue<double>("Четверть Слева"));
            
            return windowGap;
        }
    }
}