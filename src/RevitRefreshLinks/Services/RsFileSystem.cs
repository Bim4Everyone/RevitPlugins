using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class RsFileSystem : IFileSystem {
        private readonly IReadOnlyCollection<IServerClient> _serverClients;

        public RsFileSystem(IReadOnlyCollection<IServerClient> serverClients) {
            _serverClients = serverClients ?? throw new ArgumentNullException(nameof(serverClients));
        }


        public async Task<IDirectoryModel> GetDirectoryAsync(string path) {
            Uri uri = new Uri(path.Replace(@"\", @"/"));
            IServerClient serverClient = _serverClients.FirstOrDefault(
                item => item.ServerName.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
            if(serverClient is null) {
                throw new InvalidOperationException($"Не был найден сервер \"{uri.Host}\".");
            }

            string currentFolder = Path.Combine(Path.GetDirectoryName(uri.LocalPath), Path.GetFileName(uri.LocalPath));

            if(string.IsNullOrEmpty(currentFolder)) {
                return await GetRootDirectoryAsync();
            } else {
                try {
                    var folderContents = await serverClient.GetFolderContentsAsync(currentFolder);
                    return new RsDirectoryModel(folderContents, serverClient);
                } catch(Exception) {
                    return await GetRootDirectoryAsync();
                }
            }
        }

        public async Task<IDirectoryModel> GetRootDirectoryAsync() {
            return await Task.FromResult(new RsHostRootDirectoryModel(_serverClients));
        }
    }
}
