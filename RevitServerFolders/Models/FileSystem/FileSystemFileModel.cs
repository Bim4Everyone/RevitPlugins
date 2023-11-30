using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevitServerFolders.Models.FileSystem {
    internal sealed class FileSystemFileModel : ModelObject {
        private readonly FileInfo _fileInfo;

        public FileSystemFileModel(FileInfo fileInfo) {
            _fileInfo = fileInfo;
        }

        public override string Name => _fileInfo.Name;
        public override string FullName => _fileInfo.FullName;
        public override bool IsFolder => false;
        public override bool HasChildren => false;

        public override Task<IEnumerable<ModelObject>> GetChildrenObjects() {
            return Task.FromResult(Enumerable.Empty<ModelObject>());
        }
    }
}
