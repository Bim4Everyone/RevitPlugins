using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки для задания на отверстие по пересечению элемента и плиты
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FloorPointFinder<T> : IPointFinder where T : Element {
        private readonly Clash<T, CeilingAndFloor> _clash;

        public FloorPointFinder(Clash<T, CeilingAndFloor> clash) {
            _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
        }

        public XYZ GetPoint() {
            var solid = _clash.GetIntersection();
            var maxZ = _clash.Element2.IsHorizontal()
                ? GetMaxZ(_clash.Element2.GetTopFace())
                : solid.GetOutline().MaximumPoint.Z;
            double transformedMaxZ = _clash.Element2Transform.OfPoint(new XYZ(0, 0, maxZ)).Z;
            XYZ xyPoint = GetPointXY(_clash);
            return new XYZ(xyPoint.X, xyPoint.Y, transformedMaxZ);
        }

        /// <summary>
        /// Возвращает максимальную координату Z для заданной поверхности.
        /// Алгоритм предполагает, что поверхность плоская и горизонтальная
        /// </summary>
        /// <param name="horizontalFace">Горизонтальная плоская поверхность</param>
        /// <returns></returns>
        public double GetMaxZ(Face horizontalFace) {
            return horizontalFace.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
        }

        /// <summary>
        /// Возвращает точку, X и Y координаты которой соответствуют центру пересечения оси инженерного элемента 
        /// и срединной плоскости перекрытия. В остальных случаях возвращает центр солида пересечения.
        /// Точность координаты Z не гарантируется.
        /// </summary>
        /// <param name="clash"></param>
        /// <returns></returns>
        private XYZ GetPointXY(Clash<T, CeilingAndFloor> clash) {
            if(clash is null) {
                throw new ArgumentNullException(nameof(clash));
            }

            if((clash.Element1 is MEPCurve mepCurve)
                && (mepCurve.IsVertical()
                    || !mepCurve.IsHorizontal() && clash.Element2.IsHorizontal())) {

                if(mepCurve.IsVertical()) {
                    return mepCurve.GetLine().GetEndPoint(0);
                } else {
                    //здесь инженерная система не вертикальна и не горизонтальна, а перекрытие горизонтально
                    Line mepLine = mepCurve.GetLine();
                    //удлинена осевая линия инженерной системы на 5 м в обе стороны
                    var elongatedMepLine = Line.CreateBound(
                        mepLine.GetEndPoint(0) - mepLine.Direction * 16.5,
                        mepLine.GetEndPoint(1) + mepLine.Direction * 16.5);

                    //получение верхней и нижней плоскостей перекрытия в координатах активного файла ВИС
                    var topPlane = CreateTransformedPlaneByFace(clash, clash.Element2.GetTopFace());
                    var topPoint = elongatedMepLine.GetIntersectionWithPlane(topPlane);
                    var bottomPlane = CreateTransformedPlaneByFace(clash, clash.Element2.GetBottomFace());
                    var bottomPoint = elongatedMepLine.GetIntersectionWithPlane(bottomPlane);

                    return (topPoint + bottomPoint) / 2;
                }
            } else {
                return clash.GetIntersection().ComputeCentroid();
            }
        }


        /// <summary>
        /// Возвращает точку, которая принадлежит заданной поверхности
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private XYZ GetPointFromFace(Face face) {
            if(face is null) {
                throw new ArgumentNullException(nameof(face));
            }

            return face.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0);
        }

        /// <summary>
        /// Возвращает горизонтальную плоскость, 
        /// полученную из заданной поверхности и трансформированную по заданному пересечению
        /// </summary>
        /// <param name="clash"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private Plane CreateTransformedPlaneByFace(Clash<T, CeilingAndFloor> clash, Face face) {
            if(clash is null) {
                throw new ArgumentNullException(nameof(clash));
            }

            if(face is null) {
                throw new ArgumentNullException(nameof(face));
            }

            var floorTop = clash.Element2Transform.OfPoint(GetPointFromFace(face));
            return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, floorTop);
        }
    }
}
