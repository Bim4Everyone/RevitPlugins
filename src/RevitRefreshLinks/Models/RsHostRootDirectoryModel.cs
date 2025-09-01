using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

namespace RevitRefreshLinks.Models;
internal class RsHostRootDirectoryModel : IDirectoryModel {
    private readonly IReadOnlyCollection<IServerClient> _serverClients;

    public RsHostRootDirectoryModel(IReadOnlyCollection<IServerClient> serverClients) {
        _serverClients = serverClients ?? throw new ArgumentNullException(nameof(serverClients));
        if(_serverClients.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(serverClients));
        }
    }


    public bool Exists => true;

    public string FullName => "rsn://";

    public string Name => FullName;

    public async Task<IDirectoryModel[]> GetDirectoriesAsync() {
        List<IDirectoryModel> list = [];
        foreach(var client in _serverClients) {
            list.Add(new RsRootDirectory(client));
        }
        return await Task.FromResult(list.ToArray());
    }

    public async Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption) {
        switch(searchOption) {
            case SearchOption.TopDirectoryOnly:
                return await GetDirectoriesAsync();
            case SearchOption.AllDirectories: {
                var rsRoots = await GetDirectoriesAsync();
                var list = new List<IDirectoryModel>(rsRoots);
                foreach(var item in rsRoots) {
                    list.AddRange(await item.GetDirectoriesAsync(searchOption));
                }
                return list.ToArray();
            }
            default:
                throw new InvalidOperationException($"Не поддерживаемая опция {searchOption}");
        }
    }

    public async Task<IFileModel[]> GetFilesAsync(string filter = "*.*") {
        return await Task.FromResult(Array.Empty<IFileModel>());
    }

    public async Task<IFileModel[]> GetFilesAsync(SearchOption searchOption, string filter = "*.*") {
        switch(searchOption) {
            case SearchOption.TopDirectoryOnly:
                return await GetFilesAsync(filter);
            case SearchOption.AllDirectories: {
                var rsRoots = await GetDirectoriesAsync();
                var list = new List<IFileModel>();
                foreach(var item in rsRoots) {
                    list.AddRange(await item.GetFilesAsync(searchOption, filter));
                }
                return list.ToArray();
            }
            default:
                throw new InvalidOperationException($"Не поддерживаемая опция выбора файлов {searchOption}");
        }
    }

    public async Task<IDirectoryModel> GetParentAsync() {
        return await Task.FromResult(this);
    }

    public async Task<IDirectoryModel> GetRootAsync() {
        return await Task.FromResult(this);
    }
}
