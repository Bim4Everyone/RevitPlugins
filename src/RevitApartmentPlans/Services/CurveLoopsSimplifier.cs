using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    internal class CurveLoopsSimplifier : ICurveLoopsSimplifier {
        public CurveLoopsSimplifier() {
        }


        public CurveLoop Simplify(CurveLoop curveLoop) {
            var curves = MakeList(curveLoop);
            var simplifiedLoop = new CurveLoop();
            Curve curvePrev = curves[0];
            for(int i = 1; i < curves.Count; i++) {
                // проверяем, дошли ли мы до угла
                bool isContinuation = IsContinuation(curvePrev, curves[i]);
                if(isContinuation) {
                    // объединяем линии на прямом участке
                    curvePrev = CombineCurves(curvePrev, curves[i]);
                } else {
                    // если линии не являются продолжением, значит мы дошли до угла и нужно добавить предыдущий кусок
                    simplifiedLoop.Append(curvePrev);
                    curvePrev = curves[i];
                }
                if(i == curves.Count - 1) {
                    // если мы дошли до конца списка, добавляем объединенный хвост
                    simplifiedLoop.Append(curvePrev);
                }
            }
            return simplifiedLoop;
        }


        /// <summary>
        /// Возвращает список линий замкнутого контура. 
        /// Список линий начинается с угла (или сопряжения непрямых линий, например - окружностей). 
        /// Порядок линий соответствует замкнутому контуру.
        /// </summary>
        /// <param name="curveLoop">Замкнутый контур</param>
        private IList<Curve> MakeList(CurveLoop curveLoop) {
            var curves = curveLoop.ToList();
            int startIndex = 0;
            for(int previous = 0, current = 1; current < curves.Count; current++, previous++) {
                // находим первый индекс линии, которая образует угол
                if(!IsContinuation(curves[previous], curves[current])) {
                    startIndex = current;
                    break;
                }
            }
            var result = new List<Curve>();
            for(int i = startIndex; i < curves.Count; i++) {
                result.Add(curves[i]);
            }
            for(int i = 0; i < startIndex; i++) {
                result.Add(curves[i]);
            }
            return result;
        }

        /// <summary>
        /// Проверяет, является ли вторая линия продолжением первой.
        /// </summary>
        /// <param name="first">Первая линия</param>
        /// <param name="second">Вторая линия</param>
        /// <returns>Вторая линия является продолжением первой только если обе линии - отрезки 
        /// и если начало второго отрезка - это конец первого отрезка или наоборот</returns>
        private bool IsContinuation(Curve first, Curve second) {
            if(first is null || second is null) { return false; }

            if((first is Line firstLine) && (second is Line secondLine)) {
                return firstLine.Direction.IsAlmostEqualTo(secondLine.Direction)
                    && (firstLine.GetEndPoint(0).IsAlmostEqualTo(secondLine.GetEndPoint(1))
                    || firstLine.GetEndPoint(1).IsAlmostEqualTo(secondLine.GetEndPoint(0)));
            } else {
                return false;
            }
        }

        /// <summary>
        /// Строит отрезок с начальной точкой в начале первой линии и конечной точкой в конце второй линии
        /// </summary>
        /// <param name="curveFirst">Первая линия</param>
        /// <param name="curveSecond">Вторая линия</param>
        /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
        private Curve CombineCurves(Curve curveFirst, Curve curveSecond) {
            if(curveFirst is null) { throw new ArgumentNullException(nameof(curveFirst)); }
            if(curveSecond is null) { throw new ArgumentNullException(nameof(curveSecond)); }

            return Line.CreateBound(curveFirst.GetEndPoint(0), curveSecond.GetEndPoint(1));
        }
    }
}
