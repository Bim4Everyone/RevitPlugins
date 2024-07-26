using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB.Architecture;

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
        /// <param name="rooms">Помещения для отделки</param>
        /// <param name="settings">Настройки отделки стен</param>
        /// <param name="progress">Уведомитель процесса</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Ошибки в построении отделочных стен в помещениях</returns>
        ICollection<RoomErrorsViewModel> CreateWallsFinishing(
            ICollection<Room> rooms,
            RevitSettings settings,
            IProgress<int> progress = null,
            CancellationToken ct = default);
    }
}
