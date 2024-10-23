using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal abstract class OutcomingTaskOffsetFinder<T> : IOutcomingTaskOffsetFinder<T> where T : Element {
        protected OutcomingTaskOffsetFinder(
            OpeningConfig openingConfig,
            OutcomingTaskGeometryProvider geometryProvider,
            GeometryUtils geometryUtils,
            ILengthConverter lengthConverter) {

            OpeningConfig = openingConfig ?? throw new System.ArgumentNullException(nameof(openingConfig));
            GeometryProvider = geometryProvider ?? throw new System.ArgumentNullException(nameof(geometryProvider));
            GeometryUtils = geometryUtils ?? throw new System.ArgumentNullException(nameof(geometryUtils));
            LengthConverter = lengthConverter ?? throw new System.ArgumentNullException(nameof(lengthConverter));
        }


        protected OutcomingTaskGeometryProvider GeometryProvider { get; }

        protected GeometryUtils GeometryUtils { get; }

        protected OpeningConfig OpeningConfig { get; }

        protected ILengthConverter LengthConverter { get; }

        /// <summary>
        /// Количество точек тесселяции для изогнутых линий
        /// </summary>
        protected abstract int TessellationCount { get; }


        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, T mepElement) {
            var leftPlane = GeometryProvider.GetLeftPlane(opening);
            var rightPlane = GeometryProvider.GetRightPlane(opening);
            var face = GetPlanarFaceForCalculation(opening, mepElement);
            var points = GeometryUtils.GetPoints(face, TessellationCount);

            var leftDist = GeometryUtils.GetMinDistance(leftPlane, points);
            var rightDist = GeometryUtils.GetMinDistance(rightPlane, points);

            return leftDist + rightDist;
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, T mepElement) {
            var bottomPlane = GeometryProvider.GetBottomPlane(opening);
            var topPlane = GeometryProvider.GetTopPlane(opening);
            var face = GetPlanarFaceForCalculation(opening, mepElement);
            var points = GeometryUtils.GetPoints(face, TessellationCount);

            var bottomDist = GeometryUtils.GetMinDistance(bottomPlane, points);
            var topDist = GeometryUtils.GetMinDistance(topPlane, points);

            return bottomDist + topDist;
        }

        public double GetMinHorizontalOffsetSum(T mepElement) {
            var offset = GetOffset(mepElement, GetWidth(mepElement));
            var tolerance = GetTolerance(mepElement);
            return offset > tolerance ? offset - tolerance : 0;
        }

        public double GetMaxHorizontalOffsetSum(T mepElement) {
            var offset = GetOffset(mepElement, GetWidth(mepElement));
            var tolerance = GetTolerance(mepElement);
            return offset + tolerance;
        }

        public double GetMinVerticalOffsetSum(T mepElement) {
            var offset = GetOffset(mepElement, GetHeight(mepElement));
            var tolerance = GetTolerance(mepElement);
            return offset > tolerance ? offset - tolerance : 0;
        }

        public double GetMaxVerticalOffsetSum(T mepElement) {
            var offset = GetOffset(mepElement, GetHeight(mepElement));
            var tolerance = GetTolerance(mepElement);
            return offset + tolerance;
        }

        protected Solid GetIntersectionSolid(OpeningMepTaskOutcoming opening, T mepElement) {
            var intersection = GetMepSolid(mepElement);
            var frontFace = GeometryProvider.GetFrontPlane(opening);
            var backFace = GeometryProvider.GetBackPlane(opening);

            BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(intersection, frontFace);
            BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(intersection, backFace);
            return intersection;
        }

        /// <summary>
        /// Возвращает точность единицах Revit (футах)
        /// </summary>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <returns>Точность в единицах Revit</returns>
        protected double GetTolerance(T mepElement) {
            return LengthConverter.ConvertToInternal(GetCategory(mepElement).Rounding);
        }

        /// <summary>
        /// Находит сечение элемента ВИС плоскостью задания на отверстие, по которому будем определять отступы.
        /// </summary>
        /// <param name="opening">Задание на отверстие</param>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <returns>Сечение элемента ВИС в плоскости задания на отверстие</returns>
        protected PlanarFace GetPlanarFaceForCalculation(OpeningMepTaskOutcoming opening, T mepElement) {
            var frontFaceNormal = GeometryProvider.GetFrontPlane(opening).Normal;
            return GetIntersectionSolid(opening, mepElement)
                .Faces
                .OfType<Face>()
                .Where(f => f is PlanarFace face
                    && face.FaceNormal.CrossProduct(frontFaceNormal).IsAlmostEqualTo(XYZ.Zero))
                .OrderByDescending(f => f.Area)
                .FirstOrDefault() as PlanarFace
                ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Находит требуемый отступ в единицах Revit (футах) для заданного размера элемента ВИС с двух сторон
        /// </summary>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <param name="size">Размер элемента ВИС единицах Revit (футах)</param>
        /// <returns>Требуемый отступ суммарно с двух сторон от элемента ВИС единицах Revit (футах)</returns>
        protected double GetOffset(T mepElement, double size) {
            return GetCategory(mepElement).GetOffsetValue(size);
        }

        /// <summary>
        /// Находит высоту элемента ВИС в единицах Revit
        /// </summary>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <returns>Высота в единицах Revit</returns>
        protected abstract double GetHeight(T mepElement);

        /// <summary>
        /// Находит ширину элемента ВИС в единицах Revit
        /// </summary>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <returns>Ширина в единицах Revit</returns>
        protected abstract double GetWidth(T mepElement);

        /// <summary>
        /// Возвращает солид элемента ВИС для получения отступов
        /// </summary>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <returns>Солид элемента ВИС</returns>
        protected abstract Solid GetMepSolid(T mepElement);

        protected abstract MepCategory GetCategory(T mepElement);
    }
}
