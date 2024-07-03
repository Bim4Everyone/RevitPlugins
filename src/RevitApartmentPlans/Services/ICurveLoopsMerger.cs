using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Сервис для объединения замкнутых контуров в один внешний контур
    /// </summary>
    internal interface ICurveLoopsMerger {
        /// <summary>
        /// Объединяет заданную коллекцию замкнутых контуров в один внешний контур. 
        /// При этом эти контуры могут не пересекаются друг с другом.
        /// </summary>
        /// <param name="curveLoops">Коллекция замкнутых контуров, 
        /// которые надо объединить и получить один внешний контур</param>
        /// <returns>Объединенный внешний контур</returns>
        CurveLoop Merge(ICollection<CurveLoop> curveLoops);
    }
}
