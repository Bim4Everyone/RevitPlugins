using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Сервис, предоставляющий методы для оффсета замкнутых плоских контуров
    /// </summary>
    internal interface ICurveLoopsOffsetter {
        /// <summary>
        /// Создает замкнутый контур с заданным оффсетом исходного контура в горизонтальной плоскости.<br/>
        /// Если оффсет не удалось создать по исходному контуру, будет создан оффсет по прямоугольнику, 
        /// который описывает исходный контур.
        /// </summary>
        CurveLoop CreateOffsetLoop(CurveLoop curveLoop, double feetOffset);
    }
}
