using System.Collections.Generic;
using System.Threading.Tasks;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services {
    internal sealed class RsModelObjectService : IModelObjectService {
        public Task<IEnumerable<ModelObject>> OpenModelObjectDialog() {
            throw new System.NotImplementedException();
        }
    }
}
