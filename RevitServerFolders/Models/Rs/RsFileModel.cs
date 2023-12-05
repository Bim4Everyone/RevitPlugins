using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Utils;

namespace RevitServerFolders.Models.Rs {
    internal sealed class RsFileModel : ModelObject {
        private readonly ModelData _modelData;
        private readonly FolderContents _folderContents;
        private readonly IServerClient _serverClient;

        public RsFileModel(ModelData modelData, FolderContents folderContents, IServerClient serverClient) {
            _modelData = modelData;
            _folderContents = folderContents;
            _serverClient = serverClient;
        }

        public override string Name => _modelData.Name;
        public override string FullName => Extensions.GetVisibleModelPath(_serverClient, _folderContents, _modelData);
        public override bool IsFolder => false;
        public override bool HasChildren => false;

        public override Task<IEnumerable<ModelObject>> GetChildrenObjects() {
            return Task.FromResult(Enumerable.Empty<ModelObject>());
        }
    }
}
