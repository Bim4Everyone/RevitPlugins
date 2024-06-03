using System.Collections.Generic;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.ViewModels;

namespace RevitFinishingWalls.Services.Creation {
    /// <summary>
    /// Сервис, создающий отделку помещений
    /// </summary>
    internal interface IRoomFinisher {
        /// <summary>
        /// Создает отделку стен
        /// </summary>
        /// <param name="config">Настройки отделки стен</param>
        /// <returns>Ошибки в построении отделочных стен в помещениях</returns>
        ICollection<RoomErrorsViewModel> CreateWallsFinishing(PluginConfig config);
    }
}
