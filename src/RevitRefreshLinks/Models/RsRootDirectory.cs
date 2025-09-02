using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

namespace RevitRefreshLinks.Models;
/// <summary>
/// Корневая директория определенного Revit сервера
/// </summary>
internal class RsRootDirectory : IDirectoryModel {
    private readonly IServerClient _serverClient;

    public RsRootDirectory(IServerClient serverClient) {
        _serverClient = serverClient ?? throw new ArgumentNullException(nameof(serverClient));
        FullName = new UriBuilder("rsn", _serverClient.ServerName).Uri.ToString();
        Name = _serverClient.ServerName;
    }

    public bool Exists => true;

    public string FullName { get; }

    public string Name { get; }

    public async Task<IDirectoryModel[]> GetDirectoriesAsync() {
        var contents = await _serverClient.GetRootFolderContentsAsync();
        List<IDirectoryModel> dirs = [];
        foreach(var item in contents.Folders) {
            var content = await _serverClient.GetFolderContentsAsync(item.Name);
            dirs.Add(new RsDirectoryModel(content, _serverClient));
        }
        return dirs.ToArray();
    }

    public async Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption) {
        switch(searchOption) {
            case SearchOption.TopDirectoryOnly:
                return await GetDirectoriesAsync();
            case SearchOption.AllDirectories: {
                var list = new List<IDirectoryModel>();
                var contents = await _serverClient.GetRecursiveFolderContentsAsync();
                return contents.Select(f => new RsDirectoryModel(f, _serverClient)).ToArray();
            }
            default:
                throw new InvalidOperationException($"{searchOption}");
        }
    }

    public async Task<IFileModel[]> GetFilesAsync(string filter = "*.*") {
        var contents = await _serverClient.GetRootFolderContentsAsync();
        return contents.Models.Select(m => new RsFileModel(m, this)).ToArray();
    }

    public async Task<IFileModel[]> GetFilesAsync(SearchOption searchOption, string filter = "*.*") {
        switch(searchOption) {
            case SearchOption.TopDirectoryOnly:
                return await GetFilesAsync(filter);
            case SearchOption.AllDirectories: {
                var list = new List<IDirectoryModel>();
                var contents = await _serverClient.GetRecursiveFolderContentsAsync();
                return contents.Select(c => new { c.Models, FolderContents = c })
                    .SelectMany(fc => fc.Models.Select(
                        m => new RsFileModel(m, new RsDirectoryModel(fc.FolderContents, _serverClient))))
                    .ToArray();
            }
            default:
                throw new InvalidOperationException($"{searchOption}");
        }
    }

    public async Task<IDirectoryModel> GetParentAsync() {
        return await Task.FromResult(this);
    }

    public async Task<IDirectoryModel> GetRootAsync() {
        return await Task.FromResult(this);
    }
}
