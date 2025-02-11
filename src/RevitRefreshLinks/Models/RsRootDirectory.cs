using System;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

namespace RevitRefreshLinks.Models {
    /// <summary>
    /// Корневая директория определенного Revit сервера
    /// </summary>
    internal class RsRootDirectory : RsDirectoryModel {
        private readonly FolderContents _folderContents;
        private readonly IServerClient _serverClient;

        public RsRootDirectory(FolderContents folderContents, IServerClient serverClient) : base(folderContents, serverClient) {
            _folderContents = folderContents ?? throw new ArgumentNullException(nameof(folderContents));
            _serverClient = serverClient ?? throw new ArgumentNullException(nameof(serverClient));
            Name = _serverClient.ServerName;
        }

        public override bool Exists => true;

        public override string FullName => _folderContents.Path;

        public override string Name { get; }


        public override async Task<IDirectoryModel> GetParentAsync() {
            return await Task.FromResult(this);
        }

        public override async Task<IDirectoryModel> GetRootAsync() {
            return await Task.FromResult(this);
        }
    }
}
