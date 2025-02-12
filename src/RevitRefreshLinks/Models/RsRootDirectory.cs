using System;
using System.IO;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;

namespace RevitRefreshLinks.Models {
    /// <summary>
    /// Корневая директория определенного Revit сервера
    /// </summary>
    internal class RsRootDirectory : IDirectoryModel {
        private readonly IServerClient _serverClient;

        public RsRootDirectory(IServerClient serverClient) {
            _serverClient = serverClient ?? throw new ArgumentNullException(nameof(serverClient));
            Name = _serverClient.ServerName;
        }

        public bool Exists => true;

        public string FullName => _serverClient.ServerName;

        public string Name { get; }

        public Task<IDirectoryModel[]> GetDirectoriesAsync() {
            throw new NotImplementedException();
        }

        public Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption) {
            throw new NotImplementedException();
        }

        public Task<IFileModel[]> GetFilesAsync() {
            throw new NotImplementedException();
        }

        public Task<IFileModel[]> GetFilesAsync(SearchOption searchOption) {
            throw new NotImplementedException();
        }

        public async Task<IDirectoryModel> GetParentAsync() {
            return await Task.FromResult(this);
        }

        public async Task<IDirectoryModel> GetRootAsync() {
            return await Task.FromResult(this);
        }
    }
}
