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
        void CreateSlopes(PluginConfig config, out string error);
    }
}
