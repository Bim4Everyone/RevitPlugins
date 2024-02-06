using System.IO;

namespace RevitMepTotals.Services {
    /// <summary>
    /// Сервис, предоставляющий директории
    /// </summary>
    internal interface IDirectoryProvider {
        /// <summary>
        /// Предоставляет директорию, выбранную пользователем
        /// </summary>
        /// <returns></returns>
        DirectoryInfo GetDirectory();
    }
}
