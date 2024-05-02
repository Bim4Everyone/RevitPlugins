using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Exceptions;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class LineExtension {
        public static bool IsHorizontal(this Line line) {
            return Math.Abs(line.GetEndPoint(0).Z - line.GetEndPoint(1).Z) < 0.0001;
        }

        public static bool IsVertical(this Line line) {
            return Math.Abs(Math.Abs(line.Direction.Z) - 1) < 0.0001;
        }

        public static bool IsPerpendicular(this Line line, Line otherLine) {
            return line.Direction.IsPerpendicular(otherLine.Direction);
        }

        public static bool IsPerpendicular(this Line line, XYZ otherLine) {
            return line.Direction.IsPerpendicular(otherLine);
        }

        public static bool IsParallel(this Line line, Line otherLine) {
            return line.Direction.IsParallel(otherLine.Direction);
        }

        public static bool RunAlongWall(this Line line, Wall wall) {
            return line.Direction.RunAlongWall(wall);
        }

        public static Line GetTransformedLine(this Line line, Transform transform) {
            return Line.CreateBound(transform.OfPoint(line.GetEndPoint(0)), transform.OfPoint(line.GetEndPoint(1)));
        }

        public static XYZ GetIntersectionWithFace(this Line line, Face face) {
            face.Intersect(line, out IntersectionResultArray result);
            return result?.get_Item(0)?.XYZPoint ?? throw new IntersectionNotFoundException("Линия не пересекает поверхность");
        }

        public static Line GetLineWithOffset(this Line line, XYZ dir, double distance) {
            return Line.CreateBound(line.GetEndPoint(0) + distance * dir, line.GetEndPoint(1) + distance * dir);
        }

        public static XYZ GetPointFromLineEquation(this Line mepLine, Line wallLine) {
            //Получение проекции точки вставки на плоскость xOy (то есть координаты x и y точки вставки)
            var xy = GetHorizontalProjectionIntersection(mepLine, wallLine);
            double z;

            //Подстановка получившихся значений в уравнение прямой в пространстве
            if(Math.Abs(mepLine.Direction.X) > 0.0001) {
                z = (xy.X - mepLine.GetEndPoint(0).X) / mepLine.Direction.X * mepLine.Direction.Z + mepLine.GetEndPoint(0).Z;
            } else {
                z = (xy.Y - mepLine.GetEndPoint(0).Y) / mepLine.Direction.Y * mepLine.Direction.Z + mepLine.GetEndPoint(0).Z;
            }
            return new XYZ(xy.X, xy.Y, z);
        }

        private static XYZ GetHorizontalProjectionIntersection(this Line mepLine, Line wallLine) {
            var mepLineStart = new XYZ(mepLine.GetEndPoint(0).X, mepLine.GetEndPoint(0).Y, 0);
            var mepLineEnd = new XYZ(mepLine.GetEndPoint(1).X, mepLine.GetEndPoint(1).Y, 0);
            var projectedMepLine = Line.CreateBound(mepLineStart, mepLineEnd);

            var wallLineStart = new XYZ(wallLine.GetEndPoint(0).X, wallLine.GetEndPoint(0).Y, 0);
            var wallLineEnd = new XYZ(wallLine.GetEndPoint(1).X, wallLine.GetEndPoint(1).Y, 0);
            var projectedWallLIne = Line.CreateBound(wallLineStart, wallLineEnd);

            projectedMepLine.Intersect(projectedWallLIne, out IntersectionResultArray result);

            return result?.get_Item(0)?.XYZPoint ?? throw new IntersectionNotFoundException("Линии не пересекаются");
        }

        public static XYZ GetIntersectionWithFaceFromEquation(this Line line, Face face) {
            //уравнение плоскости ax+by+cz+d=0
            var faceNormal = face.ComputeNormal(new UV(0.5, 0.5));
            var a = faceNormal.X;
            var b = faceNormal.Y;
            var c = faceNormal.Z;

            var pointOnFace = face.Evaluate(new UV(0, 0));

            var d = -(a * pointOnFace.X + b * pointOnFace.Y + c * pointOnFace.Z);

            //уравнение прямой в пространстве (x-x0)/p1 = (y-y0)/p2 = (z-z0)/p3
            var pointOnLine = line.GetEndPoint(0);
            var x0 = pointOnLine.X;
            var y0 = pointOnLine.Y;
            var z0 = pointOnLine.Z;

            var p1 = line.Direction.X;
            var p2 = line.Direction.Y;
            var p3 = line.Direction.Z;

            var x = (x0 * (p2 * b + p1 * p3 * c) - p1 * (d + b * y0 + z0 * c)) / (p1 * p3 * c + b * p2 + a * p1);
            double y;
            double z;

            if(Math.Abs(p1) < 0.0001) {
                y = (c * p3 / p2 * y0 - a * x0 - c * z0 - d) / (b + c * p3 / p2);
                z = p3 / p2 * (y - y0) + z0;
            } else {
                y = p2 * (x - x0) / p1 + y0;
                z = p3 / p1 * (x - x0) + z0;
            }
            return new XYZ(x, y, z);
        }

        public static XYZ GetIntersectionWithFaceFromPlane(this Line line, Face face) {
            var planarFace = (PlanarFace) face;
            var plane = Plane.CreateByNormalAndOrigin(planarFace.FaceNormal, planarFace.Origin);

            return line.GetIntersectionWithPlane(plane);
        }

        public static XYZ GetIntersectionWithPlane(this Line line, Plane plane) {
            var point1 = plane.ProjectPoint(line.GetEndPoint(0));
            var point2 = plane.ProjectPoint(line.GetEndPoint(1));
            if(point1.DistanceTo(point2) < 0.00001) {
                return point1;
            }

            var projectedLine = Line.CreateBound(point1, point2);

            line.Intersect(projectedLine, out IntersectionResultArray result);
            return result?.get_Item(0)?.XYZPoint ?? throw new IntersectionNotFoundException("Линия параллельна плоскости");
        }
    }
}
