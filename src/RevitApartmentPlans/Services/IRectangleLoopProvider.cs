using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Сервис, предоставляющий методы для создания прямоугольных замкнутых плоских контуров, 
    /// описывающих заданные контуры
    /// </summary>
    internal interface IRectangleLoopProvider {
        /// <summary>
        /// Создает прямоугольный замкнутый наружный контур, в который вписаны все заданные замкнутые контуры.<br/>
        /// Линии в этом контуре ориентированы против часовой стрелки.
        /// </summary>
        CurveLoop CreateRectCounterClockwise(ICollection<CurveLoop> curveLoops);

        /// <summary>
        /// Создает прямоугольный замкнутый наружный контур, в который вписан заданный замкнутый контур.<br/>
        /// Линии в этом контуре ориентированы против часовой стрелки.
        /// </summary>
        CurveLoop CreateRectCounterClockwise(CurveLoop curveLoop);
    }
}
