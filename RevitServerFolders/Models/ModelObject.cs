using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitServerFolders.Models {
    internal abstract class ModelObject {
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract bool IsFolder { get; }
        public abstract bool HasChildren { get; }

        public abstract Task<IEnumerable<ModelObject>> GetChildrenObjects();
    }
}
