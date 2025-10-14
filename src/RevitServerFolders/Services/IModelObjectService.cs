using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
internal interface IModelObjectService : IAttachableService {
    Task<ModelObject> SelectModelObjectDialog();
    Task<ModelObject> SelectModelObjectDialog(string rootFolder);
    Task<ModelObject> GetFromString(string folderName);
}
