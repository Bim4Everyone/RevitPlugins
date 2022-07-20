using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class WallAngleFinder : IAngleFinder {
        private readonly Wall _wall;

        public WallAngleFinder(Wall wall) {
            _wall = wall;
        }

        public double GetAngle() {
            var orientation = _wall.Orientation;
            return - orientation.AngleTo(XYZ.BasisY);
        }
    }
}
