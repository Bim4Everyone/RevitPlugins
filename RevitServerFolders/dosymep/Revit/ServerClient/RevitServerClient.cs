using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dosymep.Revit.ServerClient {
    /// <summary>
    /// Клиент доступа к Revit сервер.
    /// </summary>
    public class RevitServerClient : IRevitServerClient {
        private readonly ISerializer _serializer;
        private readonly IRevitHttpClient _revitHttpClient;

        /// <summary>
        /// Создает экземпляр класса клиента Revit сервера.
        /// </summary>
        /// <param name="serverName">Наименование Revit сервера.</param>
        /// <param name="serverVersion">Версия Revit сервера.</param>
        public RevitServerClient(string serverName, string serverVersion, ISerializer serializer) {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _revitHttpClient = new RevitHttpClient(serverName, serverVersion);
        }

        /// <inheritdoc>/>
        public string ServerName {
            get { return _revitHttpClient.ServerName; } 
        }

        /// <inheritdoc>/>
        public string ServerVersion {
            get { return _revitHttpClient.ServerVersion; }
        }

        /// <inheritdoc>/>
        public async Task<ServerInformations> GetServerPropertiesAsync(CancellationToken cancellationToken = default) {
            var response = await _revitHttpClient.Get("serverProperties", cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            return _serializer.Deserialize<ServerInformations>(result);
        }

        /// <inheritdoc>/>
        public Task<RevitContents> GetRootContentsAsync(CancellationToken cancellationToken = default) {
            return GetContentsAsync("|", cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<RevitContents> GetContentsAsync(string folderPath, CancellationToken cancellationToken = default) {
            if(string.IsNullOrEmpty(folderPath)) {
                throw new ArgumentException($"'{nameof(folderPath)}' cannot be null or empty.", nameof(folderPath));
            }

            folderPath = ReplacePathSeparator(folderPath);

            var response = await _revitHttpClient.Get(folderPath + "/contents", cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            return _serializer.Deserialize<RevitContents>(result);
        }

        /// <inheritdoc/>
        public async Task<List<RevitContents>> GetAllRevitContentsAsync(CancellationToken cancellationToken = default) {
            RevitContents revitContents = await GetRootContentsAsync(cancellationToken);

            List<RevitContents> revitContentsList = await GetRevitContentsInternalAsync(revitContents, cancellationToken);
            revitContentsList.Insert(0, revitContents);

            return revitContentsList;
        }

        /// <inheritdoc/>
        public async Task<List<RevitContents>> GetAllRevitContentsAsync(string parentPath, CancellationToken cancellationToken = default) {
            RevitContents revitContents = await GetContentsAsync(parentPath, cancellationToken);

            List<RevitContents> revitContentsList = await GetRevitContentsInternalAsync(revitContents, cancellationToken);
            revitContentsList.Insert(0, revitContents);

            return revitContentsList;
        }

        /// <inheritdoc/>
        public Task<DirectoryInfo> GetRootDirectoryInfoAsync(CancellationToken cancellationToken = default) {
            return GetDirectoryInfoAsync("|", cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<DirectoryInfo> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default) {
            if(string.IsNullOrEmpty(path)) {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            path = ReplacePathSeparator(path);

            var response = await _revitHttpClient.Get(path + "/DirectoryInfo", cancellationToken);
            var result = await response.Content.ReadAsStringAsync();

            return _serializer.Deserialize<DirectoryInfo>(result);
        }

        private async Task<List<RevitContents>> GetRevitContentsInternalAsync(RevitContents parentContents, CancellationToken cancellationToken = default) {
            var revitContentsList = new List<RevitContents>();

            foreach(RevitFolder revitFolder in parentContents.Folders) {
                cancellationToken.ThrowIfCancellationRequested();

                if(!revitFolder.HasContents) {
                    continue;
                }

                RevitContents revitContents = await GetContentsAsync(parentContents.Path + "|" + revitFolder.Name, cancellationToken);
                revitContentsList.Add(revitContents);

                List<RevitContents> childrenContents = await GetRevitContentsInternalAsync(revitContents, cancellationToken);
                revitContentsList.AddRange(childrenContents);
            }

            return revitContentsList;
        }

        private static string ReplacePathSeparator(string folderName) {
            return folderName.Replace('\\', '|').Replace('/', '|');
        }
    }
}
