using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Utils;

namespace RevitServerFolders.Models.Rs {
    internal sealed class RsFolderModel : ModelObject {
        private readonly FolderData _folderData;
        private readonly FolderContents _folderContents;
        private readonly IServerClient _serverClient;

        public RsFolderModel(FolderData folderData, FolderContents folderContents, IServerClient serverClient) {
            _folderData = folderData;
            _folderContents = folderContents;
            _serverClient = serverClient;
        }

        public override string Name => _folderData.Name;
        public override string FullName => _serverClient.GetVisibleModelPath(_folderContents, _folderData);
        public override bool IsFolder => true;
        public override bool HasChildren => _folderData.HasContents;

        public override async Task<IEnumerable<ModelObject>> GetChildrenObjects() {
            string relativePath = _folderContents.GetRelativeModelPath(_folderData);
            List<FolderContents> folderContents = await _serverClient.GetRecursiveFolderContentsAsync(relativePath);
            IEnumerable<ModelObject> rsFileModels = folderContents.SelectMany(contents => contents.Models
                .Select(modelData => new RsFileModel(modelData, contents, _serverClient)));

            return rsFileModels.ToArray().AsEnumerable();
        }
    }
}
