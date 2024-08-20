using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders {
    internal class OpeningGroupSolidProvider : ISolidProvider {
        private readonly OpeningsGroup _openingsGroup;

        public OpeningGroupSolidProvider(OpeningsGroup openingsGroup) {
            _openingsGroup = openingsGroup;
        }
        public Solid GetSolid() {
            var transform = _openingsGroup.Elements.First().GetFamilyInstance().GetTotalTransform();
            return _openingsGroup.Elements.Select(item => SolidUtils.CreateTransformed(item.GetSolid(), transform.Inverse))
               .Select(item => item.GetTransformedBoundingBox())
               .ToList()
               .CreateUnitedBoundingBox()
               .CreateSolid();
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            throw new System.NotImplementedException();
        }
    }
}
