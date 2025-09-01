using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class RsFileSystem : IFileSystem {
    private readonly IReadOnlyCollection<IServerClient> _serverClients;

    public RsFileSystem(IReadOnlyCollection<IServerClient> serverClients) {
        _serverClients = serverClients ?? throw new ArgumentNullException(nameof(serverClients));
    }


    public async Task<IDirectoryModel> GetDirectoryAsync(string path) {
        var uri = new Uri(path.Replace(@"\", @"/"));
        var serverClient = _serverClients.FirstOrDefault(
            item => item.ServerName.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
        if(serverClient is null) {
            return await GetRootDirectoryAsync();
        }

        string parentName = Path.GetDirectoryName(uri.LocalPath);
        string fileName = Path.GetFileName(uri.LocalPath);
        string currentFolder = null;
        if(!string.IsNullOrWhiteSpace(parentName) && !string.IsNullOrWhiteSpace(fileName)) {
            currentFolder = Path.Combine(parentName, fileName);
        }

        if(string.IsNullOrWhiteSpace(currentFolder)) {
            return new RsRootDirectory(serverClient);
        } else {
            try {
                var folderContents = await serverClient.GetFolderContentsAsync(currentFolder);
                return new RsDirectoryModel(folderContents, serverClient);
            } catch(Exception) {
                return new RsRootDirectory(serverClient);
            }
        }
    }

    public async Task<IDirectoryModel> GetRootDirectoryAsync() {
        return await Task.FromResult(new RsHostRootDirectoryModel(_serverClients));
    }
}
