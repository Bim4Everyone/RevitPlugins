using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Projectors {
    internal class XoYProjection : IProjector {
        public double GetAngleOnPlaneToAxis(XYZ xyz) {
            return XYZ.BasisY.AngleOnPlaneTo(xyz, XYZ.BasisZ);
        }

        public XYZ ProjectPoint(XYZ xyz) {
            return xyz.ProjectOnXoY();
        }

        public XYZ ProjectVector(XYZ xyz) {
            return xyz.ProjectOnXoY();
        }
    }
}
