using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RoundMepDirsGetter : IDirectionsGetter {
        private readonly MepCurveClash<Wall> _clash;

        public RoundMepDirsGetter(MepCurveClash<Wall> clash) {
            _clash = clash;
        }

        public IEnumerable<XYZ> GetDirectionsOnPlane(Plane plane) {
            //получение осевой линии инженерной системы в координатах связанного файла со стенами
            var transformedMepLine = _clash.GetTransformedMepLine();

            //получение угла между проекцией осевой линии на плоскость и осью Y на этой плоскости
            var angle = plane.GetAngleOnPlaneToYAxis(transformedMepLine.Direction);
            XYZ dir;

            //нахождение перпендикулрного вектора осевой линии на плоскости
            if(Math.Abs(Math.Cos(angle)) < 0.0001) {
                //если угол равен 90 градусов, значит, что смещать осевую линию на плоскости нужно в направлении оси Y 
                dir = plane.YVec;
            } else {
                dir = plane.ProjectVector(transformedMepLine.Direction).CrossProduct(plane.Normal).Normalize();
            }

            yield return dir;
            yield return -dir;
        }
    }
}
