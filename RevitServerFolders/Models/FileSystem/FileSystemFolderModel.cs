using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevitServerFolders.Models.FileSystem {
    internal sealed class FileSystemFolderModel : ModelObject {
        private readonly DirectoryInfo _directoryInfo;

        public FileSystemFolderModel(DirectoryInfo directoryInfo) {
            _directoryInfo = directoryInfo;
        }

        public override string Name => _directoryInfo.Name;
        public override string FullName => _directoryInfo.FullName;
        public override bool IsFolder => true;
        public override bool HasChildren => true;

        public override Task<IEnumerable<ModelObject>> GetChildrenObjects() {
            return Task.Run(() => {
                IEnumerable<ModelObject> modelOObjects = _directoryInfo.GetFiles("*.rvt", SearchOption.AllDirectories)
                    .Select(item => new FileSystemFileModel(item));

                return modelOObjects.ToArray().AsEnumerable();
            });
        }
    }
}
