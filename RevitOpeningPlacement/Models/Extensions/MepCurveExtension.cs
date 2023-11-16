using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class MepCurveExtension {
        public static Line GetLine(this MEPCurve curve) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            var elevations = curve.GetElevations().ToList();
            var start = FixElevation(line.GetEndPoint(0), elevations);
            var end = FixElevation(line.GetEndPoint(1), elevations);

            return Line.CreateBound(start, end);
        }

        public static bool IsHorizontal(this MEPCurve curve) {
            return curve.GetLine().IsHorizontal();
        }

        public static bool IsVertical(this MEPCurve curve) {
            return curve.GetLine().IsVertical();
        }

        public static bool IsPerpendicular(this MEPCurve curve, Wall wall) {
            return curve.GetLine().IsPerpendicular(wall.GetLine())
                && curve.GetLine().IsPerpendicular(XYZ.BasisZ);
        }

        public static bool IsParallel(this MEPCurve curve, Wall wall) {
            return curve.GetLine().IsParallel(wall.GetLine());
        }

        public static bool RunAlongWall(this MEPCurve mepCurve, Wall wall) {
            return mepCurve.GetLine().RunAlongWall(wall);
        }

        public static double GetTopElevation(this MEPCurve curve) {
            return curve.GetBuiltInParam(RevitRepository.TopElevation);
        }

        public static double GetBottomElevation(this MEPCurve curve) {
            return curve.GetBuiltInParam(RevitRepository.BottomElevation);
        }

        public static double GetDiameter(this MEPCurve curve) {
            return curve.GetBuiltInParam(RevitRepository.MepCurveDiameters);
        }

        public static double GetHeight(this MEPCurve curve) {
            return curve.GetBuiltInParam(RevitRepository.MepCurveHeights);
        }

        public static double GetWidth(this MEPCurve curve) {
            return curve.GetBuiltInParam(RevitRepository.MepCurveWidths);
        }

        public static Transform GetConnectorCoordinateSystem(this MEPCurve curve) {
            var connectorsEnumerator = curve.ConnectorManager.Connectors.GetEnumerator();
            connectorsEnumerator.MoveNext();
            var connector = connectorsEnumerator.Current as Connector;
            return connector.CoordinateSystem;
        }

        public static Level GetLevel(this MEPCurve curve) {
            var levelId = curve.GetParamValueOrDefault<ElementId>(BuiltInParameter.RBS_START_LEVEL_PARAM);
            if(levelId.IsNull()) {
                return null;
            }
            return (Level) curve.Document.GetElement(levelId);
        }

        public static double GetConnectorArea(this MEPCurve curve) {
            var connector = curve.ConnectorManager.Connectors.OfType<Connector>().First();
            return connector.GetArea();
        }

        private static double GetBuiltInParam(this MEPCurve curve, IEnumerable<BuiltInParameter> parameters) {
            foreach(var param in parameters) {
                if(curve.IsExistsParam(param)) {
                    return curve.GetParamValueOrDefault<double>(param);
                }
            }
            return 0;
        }

        private static double GetOffset(this MEPCurve curve) {

            var bottomElevation = curve.GetBottomElevation();
            var topElevation = curve.GetTopElevation();
            var center = curve.GetParamValueOrDefault<double>(BuiltInParameter.RBS_OFFSET_PARAM);
            return new[] { bottomElevation, topElevation }.Min(item => Math.Abs(item - center));
        }

        private static IEnumerable<double> GetElevations(this MEPCurve curve) {
            var offset = curve.GetOffset();
            double levelElevation = curve.GetLevel()?.ProjectElevation ?? 0;

            yield return curve.GetTopElevation() - offset + levelElevation;
            yield return curve.GetBottomElevation() + offset + levelElevation;
        }

        private static XYZ FixElevation(XYZ xyz, IEnumerable<double> elevations) {
            var elevation = elevations.FirstOrDefault(item => Math.Abs(item - xyz.Z) == elevations.Min(e => Math.Abs(e - xyz.Z)));
            return new XYZ(xyz.X, xyz.Y, elevation);
        }
    }
}
