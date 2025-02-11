using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Services {
    /// <summary>
    /// Сервис, предоставляющий методы по упрощению замкнутых контуров
    /// </summary>
    internal interface ICurveLoopsSimplifier {
        /// <summary>
        /// Возвращает упрощенный замкнутый контур, 
        /// в котором линии, лежащие на одной прямой и являющиеся продолжением друг друга объединены в одну.
        /// </summary>
        /// <param name="curveLoop">Замкнутый контур</param>
        /// <returns>Новый контур с объединенными линиями</returns>
        CurveLoop Simplify(CurveLoop curveLoop);
    }
}
