using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation {
    /// <summary>
    /// Сервис, создающий отделку помещений
    /// </summary>
    internal interface IRoomFinisher {
        /// <summary>
        /// Создает отделку стен
        /// </summary>
        /// <param name="config">Настройки отделки стен</param>
        /// <param name="error">Ошибки в построении отделочных стен</param>
        void CreateWallsFinishing(PluginConfig config, out string error);
    }
}
