using System.Collections.Generic;
using System.Threading.Tasks;

using RevitServerFolders.Models;
using RevitServerFolders.Views.Rs;

namespace RevitServerFolders.Services {
    internal sealed class RsModelObjectService : IModelObjectService {
        private readonly MainWindow _mainWindow;

        public RsModelObjectService(MainWindow mainWindow) {
            _mainWindow = mainWindow;
        }
        
        public Task<ModelObject> SelectModelObjectDialog() {
            return SelectModelObjectDialog(null);
        }

        public Task<ModelObject> SelectModelObjectDialog(string rootFolder) {
            if(_mainWindow.ShowDialog() == true) {
                return Task.FromResult((ModelObject) null);
            }

            return Task.FromResult((ModelObject) null);
        }
        
        public Task<ModelObject> GetFromString(string folderName) {
            throw new System.NotImplementedException();
        }
    }
}
