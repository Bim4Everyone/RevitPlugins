using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

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

    internal static class ServerClientExtension {
        /// <summary>Returns visible model path for RS.</summary>
        /// <param name="serverClient">Server client connection.</param>
        /// <param name="folderContents">Parent folder contents.</param>
        /// <param name="objectData">Object data.</param>
        /// <returns>Returns visible model path for RS.</returns>
        public static string GetVisibleModelPath(
            this IServerClient serverClient,
            FolderContents folderContents,
            ObjectData objectData) {
            return Path.Combine("RSN://" + serverClient.ServerName, folderContents.GetRelativeModelPath(objectData));
        }
    }
}
