using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Projectors {
    internal class PlaneProjector : IProjector {

        private Plane _plane;
        public PlaneProjector(XYZ origin, XYZ axisX, XYZ axisY) {
            InitializePlane(origin, axisX, axisY);
        }

        private void InitializePlane(XYZ origin, XYZ axisX, XYZ axisY) {
            _plane = Plane.CreateByOriginAndBasis(origin, axisX, axisY);
        }

        public double GetAngleOnPlaneToAxis(XYZ xyz) {
            return _plane.YVec.AngleOnPlaneTo(xyz, _plane.Normal);
        }

        public XYZ ProjectPoint(XYZ point) {
            return _plane.ProjectPoint(point);
        }

        public XYZ ProjectVector(XYZ vector) {
            return _plane.ProjectVector(vector);
        }

        public XYZ GetPlaneX() {
            return _plane.XVec;
        }

        public XYZ GetPlaneY() {
            return _plane.YVec;
        }
    }
}
