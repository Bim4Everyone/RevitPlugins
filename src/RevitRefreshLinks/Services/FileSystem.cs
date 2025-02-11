using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class FileSystem : IFileSystem {
        private readonly IReadOnlyCollection<IServerClient> _serverClients;

        public FileSystem(IReadOnlyCollection<IServerClient> serverClients) {
            _serverClients = serverClients ?? throw new ArgumentNullException(nameof(serverClients));
        }


        public async Task<IDirectoryModel> GetDirectory(string path) {
            Uri uri = new Uri(path.Replace(@"\", @"/"));
            IServerClient serverClient = _serverClients.FirstOrDefault(
                item => item.ServerName.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
            if(serverClient is null) {
                throw new InvalidOperationException($"Не был найден сервер \"{uri.Host}\".");
            }

            string currentFolder = Path.GetFileName(uri.LocalPath);

            if(string.IsNullOrEmpty(currentFolder)) {
                return await GetRootDirectory();
            } else {
                var folderContents = await serverClient.GetFolderContentsAsync(currentFolder);
                return new RsDirectoryModel(folderContents, serverClient);
            }
        }

        public async Task<IDirectoryModel> GetRootDirectory() {
            return await Task.FromResult(new RsHostRootDirectoryModel(_serverClients));
        }
    }
}
