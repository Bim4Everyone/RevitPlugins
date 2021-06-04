using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Интерфейс HTTP соединения с Revit сервером.
    /// </summary>
    internal interface IRevitHttpClient {
        /// <summary>
        /// Наименование Revit сервера
        /// </summary>
        string ServerName { get; }

        /// <summary>
        /// Версия Revit сервера
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Формирует GET запрос к серверу.
        /// </summary>
        /// <param name="requestUri">Ссылка запроса.</param>
        /// <param name="cancellationToken">Токен отмены запроса.</param>
        /// <returns>Возвращает результат запроса к серверу.</returns>
        Task<HttpResponseMessage> Get(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Формирует PUT запрос к серверу.
        /// </summary>
        /// <param name="requestUri">Ссылка запроса.</param>
        /// <param name="cancellationToken">Токен отмены запроса.</param>
        /// <returns>Возвращает результат запроса к серверу.</returns>
        Task<HttpResponseMessage> Put(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Формирует POST запрос к серверу.
        /// </summary>
        /// <param name="requestUri">Ссылка запроса.</param>
        /// <param name="cancellationToken">Токен отмены запроса.</param>
        /// <returns>Возвращает результат запроса к серверу.</returns>
        Task<HttpResponseMessage> Post(string requestUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Формирует DELETE запрос к серверу.
        /// </summary>
        /// <param name="requestUri">Ссылка запроса.</param>
        /// <param name="cancellationToken">Токен отмены запроса.</param>
        /// <returns>Возвращает результат запроса к серверу.</returns>
        Task<HttpResponseMessage> Delete(string requestUri, CancellationToken cancellationToken = default);
    }
}