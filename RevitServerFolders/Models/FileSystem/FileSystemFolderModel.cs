using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;

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
                IEnumerable<ModelObject> modelObjects = _directoryInfo.GetFiles("*.rvt", SearchOption.AllDirectories)
                    .Where(item => IsSupportedVersion(item))
                    .Select(item => new FileSystemFileModel(item));

                return modelObjects.ToArray().AsEnumerable();
            });
        }

        private static bool IsSupportedVersion(FileInfo item) {
            try {
                return new dosymep.Revit.FileInfo.RevitFileInfo(item.FullName)
                    .BasicFileInfo.AppInfo.Format.Equals(ModuleEnvironment.RevitVersion);
            } catch {
                return false;
            }
        }
    }
}
