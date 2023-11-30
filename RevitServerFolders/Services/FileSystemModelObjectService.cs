using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Models.FileSystem;

namespace RevitServerFolders.Services {
    internal sealed class FileSystemModelObjectService : IModelObjectService {
        private readonly IOpenFolderDialogService _openFolderDialogService;

        public FileSystemModelObjectService(IOpenFolderDialogService openFolderDialogService) {
            _openFolderDialogService = openFolderDialogService;
        }

        public Task<ModelObject> SelectModelObjectDialog() {
            _openFolderDialogService.Multiselect = false;
            _openFolderDialogService.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if(!_openFolderDialogService.ShowDialog()) {
                throw new OperationCanceledException();
            }

            IEnumerable<ModelObject> folders = _openFolderDialogService.Folders
                .Select(item => new FileSystemFolderModel(item));

            return Task.FromResult(folders.FirstOrDefault());
        }

        public Task<IEnumerable<ModelObject>> SelectModelObjectsDialog() {
            _openFolderDialogService.Multiselect = true;
            _openFolderDialogService.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if(!_openFolderDialogService.ShowDialog()) {
                throw new OperationCanceledException();
            }

            IEnumerable<ModelObject> folders = _openFolderDialogService.Folders
                .Select(item => new FileSystemFolderModel(item));

            return Task.FromResult(folders.ToArray().AsEnumerable());
        }

        public Task<ModelObject> GetFromString(string folderName) {
            if(Directory.Exists(folderName)) {
                return Task.FromResult((ModelObject) new FileSystemFolderModel(new DirectoryInfo(folderName)));
            }

            return Task.FromResult((ModelObject) default);
        }
    }
}
