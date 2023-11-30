using System.Collections.Generic;
using System.Threading.Tasks;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services {
    internal interface IModelObjectService {
        Task<IEnumerable<ModelObject>> OpenModelObjectDialog();
    }
}
