using System;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    internal class CurveLoopsOffsetter : ICurveLoopsOffsetter {
        private readonly IRectangleLoopProvider _rectangleLoopProvider;

        public CurveLoopsOffsetter(IRectangleLoopProvider rectangleLoopProvider) {
            _rectangleLoopProvider = rectangleLoopProvider
                ?? throw new ArgumentNullException(nameof(rectangleLoopProvider));
        }


        /// <summary>
        /// Создает замкнутый контур с заданным оффсетом исходного контура в горизонтальной плоскости.<br/>
        /// Если оффсет не удалось создать по исходному контуру, будет создан оффсет по прямоугольнику,
        /// который описывает исходный контур.
        /// </summary>
        public CurveLoop CreateOffsetLoop(CurveLoop curveLoop, double feetOffset) {
            try {
                // если ориентация линий в контуре против часовой стрелки, то положительный оффсет увеличивает контур
                // если ориентация линий в контуре по часовой стрелке, то отрицательный оффсет увеличивает контур
                double offset = curveLoop.IsCounterclockwise(XYZ.BasisZ)
                    ? feetOffset
                    : -feetOffset;
                return CurveLoop.CreateViaOffset(curveLoop, offset, XYZ.BasisZ);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                return CurveLoop.CreateViaOffset(
                    _rectangleLoopProvider.CreateRectCounterClockwise(curveLoop), feetOffset, XYZ.BasisZ);
            }
        }
    }
}
