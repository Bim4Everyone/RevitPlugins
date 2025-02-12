using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

namespace RevitRefreshLinks.Models {
    internal class RsDirectoryModel : IDirectoryModel {
        private readonly FolderContents _folderContents;
        private readonly IServerClient _serverClient;

        public RsDirectoryModel(FolderContents folderContents, IServerClient serverClient) {
            _folderContents = folderContents ?? throw new ArgumentNullException(nameof(folderContents));
            _serverClient = serverClient ?? throw new ArgumentNullException(nameof(serverClient));
            Name = Path.GetFileName(_folderContents.Path);
        }


        public bool Exists => true;

        public string FullName => _folderContents.Path;

        public string Name { get; }

        public async Task<IDirectoryModel[]> GetDirectoriesAsync() {
            List<IDirectoryModel> dirs = new List<IDirectoryModel>();
            foreach(var item in _folderContents.Folders) {
                dirs.Add(new RsDirectoryModel(
                    await _serverClient.GetFolderContentsAsync(Path.Combine(FullName, item.Name)),
                    _serverClient));
            }
            return dirs.ToArray();
        }

        public async Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption) {
            switch(searchOption) {
                case SearchOption.TopDirectoryOnly:
                    return await GetDirectoriesAsync();
                case SearchOption.AllDirectories: {
                    var list = new List<IDirectoryModel>();
                    var contents = await _serverClient.GetRecursiveFolderContentsAsync(FullName);
                    return contents.Select(f => new RsDirectoryModel(f, _serverClient)).ToArray();
                }
                default:
                    throw new InvalidOperationException($"Не поддерживаемая опция {searchOption}");
            }
        }

        public async Task<IFileModel[]> GetFilesAsync() {
            return await Task.FromResult(_folderContents.Models.Select(f => new RsFileModel(f, this)).ToArray());
        }

        public async Task<IFileModel[]> GetFilesAsync(SearchOption searchOption) {
            switch(searchOption) {
                case SearchOption.TopDirectoryOnly:
                    return await GetFilesAsync();
                case SearchOption.AllDirectories: {
                    var list = new List<IDirectoryModel>();
                    var contents = await _serverClient.GetRecursiveFolderContentsAsync(FullName);
                    return contents.Select(c => new { Models = c.Models, FolderContents = c })
                        .SelectMany(fc => fc.Models.Select(
                            m => new RsFileModel(m, new RsDirectoryModel(fc.FolderContents, _serverClient))))
                        .ToArray();
                }
                default:
                    throw new InvalidOperationException($"Не поддерживаемая опция выбора файлов {searchOption}");
            }
        }

        public async Task<IDirectoryModel> GetParentAsync() {
            Uri uri = new Uri(FullName.Replace('\\', '/'));
            var parent = Path.GetDirectoryName(uri.LocalPath);

            if(string.IsNullOrWhiteSpace(parent)) {
                return await GetRootAsync();
            } else {
                var contents = await _serverClient.GetFolderContentsAsync(parent);
                return new RsDirectoryModel(contents, _serverClient);
            }
        }

        public async Task<IDirectoryModel> GetRootAsync() {
            return await Task.FromResult(new RsRootDirectory(_serverClient));
        }
    }
}
