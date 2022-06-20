using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class ParentBasicWindow : ParentWindow {
        private readonly FamilyInstance _parentWindow;

        public ParentBasicWindow(FamilyInstance parentWindow, RevitRepository revitRepository)
            : base(parentWindow, revitRepository) {
            _parentWindow = parentWindow;
        }

        public override double Width
            => _parentWindow.Symbol.GetParamValue<double>(BuiltInParameter.WINDOW_WIDTH);

        public override double Height
            => _parentWindow.Symbol.GetParamValue<double>(BuiltInParameter.WINDOW_HEIGHT);

        public void SetPLaceLocation(XYZ value) {
            ((LocationPoint) _parentWindow.Location).Point = value;
        }

        protected XYZ GetPlaceLocation() {
            XYZ point = new XYZ(0, 0, Height);
            var wall = (Wall) _parentWindow.Host;

#if REVIT_2022_OR_GREATER
            if(wall.CrossSection == WallCrossSection.SingleSlanted) {
                var radian = wall.GetParamValue<double>(BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL);
                
                LocationCurve location = (LocationCurve) wall.Location;
                Line line = (Line) location.Curve;

                point = Transform.CreateRotation(line.Direction, radian).OfVector(point);
                return ((LocationPoint) _parentWindow.Location).Point.Add(point);
            }
#endif

            return ((LocationPoint) _parentWindow.Location).Point.Add(point);
        }

        protected override Element GetHostObject() {
            return _parentWindow.Host;
        }

        protected override HostObject GetNextHostObject() {
            throw new NotImplementedException();
        }

        protected override IEnumerable<HostObject> GetHostElements() {
            XYZ point = ((LocationPoint) _parentWindow.Location).Point;
            return _revitRepository.GetNearestElements((HostObject) _parentWindow.Host, point,
                ((Wall) _parentWindow.Host).Orientation);
        }

        protected override FamilyInstance UpdateParamsWindowGap(FamilyInstance windowGap) {
            windowGap.SetParamValue(BuiltInParameter.WINDOW_WIDTH, Width);
            windowGap.SetParamValue(BuiltInParameter.WINDOW_HEIGHT, Height);

            windowGap.SetParamValue("Смещение Сверху",
                _parentWindow.Symbol.GetParamValue<double>("Четверть Сверху"));

            windowGap.SetParamValue("Смещение Справа",
                _parentWindow.Symbol.GetParamValue<double>("Четверть Справа"));

            windowGap.SetParamValue("Смещение Слева",
                _parentWindow.Symbol.GetParamValue<double>("Четверть Слева"));

            return windowGap;
        }
    }
}