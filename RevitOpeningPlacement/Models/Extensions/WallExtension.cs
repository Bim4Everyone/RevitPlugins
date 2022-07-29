using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class WallExtension {
        public static Line GetLine(this Wall wall) {
            return (Line) ((LocationCurve) wall.Location).Curve;
        }

        public static Line GetСentralWallLine(this Wall wall) {
            var centralPoint = GetCentralPoint(wall);

            var wallLine = wall.GetLine();

            var bottomCentralPoint = new XYZ(centralPoint.X, centralPoint.Y, wallLine.GetEndPoint(0).Z);

            return Line.CreateBound(bottomCentralPoint + wallLine.Direction * wallLine.Length / 2,
                                    bottomCentralPoint - wallLine.Direction * wallLine.Length / 2);
        }

        public static IEnumerable<Face> GetFaces(this Wall wall) {
            var interiorFace = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            var exteriorFace = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);

            yield return (Face) wall.GetGeometryObjectFromReference(interiorFace[0]);
            yield return (Face) wall.GetGeometryObjectFromReference(exteriorFace[0]);
        }

        private static XYZ GetCentralPoint(Wall wall) {
            var bb = wall.get_BoundingBox(null);
            return bb.Min + (bb.Max - bb.Min) / 2;
        }
    }
}
