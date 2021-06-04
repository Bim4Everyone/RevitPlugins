using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    public interface IRevitServerClient {
        /// <summary>
        /// Наименование Revit сервера
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Версия Revit сервера
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Возвращает информацию о Revit сервере.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает информацию о Revit сервере.</returns>
        Task<ServerInformations> GetServerPropertiesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает список родительских элементов.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает список родительских элементов.</returns>
        Task<RevitContents> GetRootContentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает список дочерних элементов переданной папки.
        /// </summary>
        /// <param name="folderPath">Путь до запрашиваемой папки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает список дочерних элементов переданной папки.</returns>
        Task<RevitContents> GetContentsAsync(string folderPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает всю структуру Revit сервера.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает всю структуру Revit сервера по переданной папке.</returns>
        Task<List<RevitContents>> GetAllRevitContentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает всю структуру Revit сервера по переданной папке.
        /// </summary>
        /// <param name="parentPath">Путь до родительской папке.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает всю структуру Revit сервера по переданной папке.</returns>
        Task<List<RevitContents>> GetAllRevitContentsAsync(string parentPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает информацию о родительском элементе.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает информацию о родительском элементе.</returns>
        Task<DirectoryInfo> GetRootDirectoryInfoAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Возвращает информацию о переданном элементе.
        /// </summary>
        /// <param name="path">Путь до запрашиваемой элемента.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает информацию о переданном элементе.</returns>
        Task<DirectoryInfo> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default);
    }
}