using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IProjector {
        XYZ ProjectPoint(XYZ xyz);
        XYZ ProjectVector(XYZ xyz);
        double GetAngleOnPlaneToAxis(XYZ xyz);
    }
}
