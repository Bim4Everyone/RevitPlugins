using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal class PositionBuilder {
        private readonly SpotDimension _spot;
        private readonly IEnumerable<FamilySymbol> _symbols;
        private XYZ _plane;
        private FamilySymbol _familySymbol;
        private BoundingBoxXYZ _bb;

        public PositionBuilder(SpotDimension spot, IEnumerable<FamilySymbol> symbols) {
            _symbols = symbols;
            _spot = spot;
            _bb = spot.get_BoundingBox(spot.View);
        }

        public PositionBuilder SetType() {
            _familySymbol = _symbols.FirstOrDefault(item => item.Name.Equals(GetTypeName(), StringComparison.CurrentCultureIgnoreCase));
            return this;
        }

        public PositionBuilder SetPlane() {
            _plane = Math.Abs(_bb.Max.X - _bb.Min.X) > 0.01 ? new XYZ(1, 0, 0) : new XYZ(0, 1, 0);
            return this;
        }

        private string GetTypeName() {
            return _spot.Origin.Z < _bb.Max.Z ? RevitRepository.TypeTop : RevitRepository.TypeBottom;
        }

    }
}
