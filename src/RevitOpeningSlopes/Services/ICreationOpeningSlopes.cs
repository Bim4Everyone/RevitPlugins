using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using RevitOpeningSlopes.Models;

namespace RevitOpeningSlopes.Services {
    /// <summary>
    /// Сервис, размещающий откосы по окнам
    /// </summary>
    internal interface ICreationOpeningSlopes {
        /// <summary>
        /// Создает экземпляры откосов по окнам
        /// </summary>
        /// <param name="config">Настройки откосов</param>
        /// <param name="error">Ошибки в построении откосов</param>
        void CreateSlopes(PluginConfig config,
            ICollection<FamilyInstance> openings,
            out string error,
            IProgress<int> progress,
            CancellationToken ct);
    }
}
