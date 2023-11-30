using System;
using System.Collections.Generic;
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

        public Task<IEnumerable<ModelObject>> OpenModelObjectDialog() {
            _openFolderDialogService.Multiselect = true;
            _openFolderDialogService.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if(_openFolderDialogService.ShowDialog()) {
                IEnumerable<ModelObject> folders = _openFolderDialogService.Folders
                    .Select(item => new FileSystemFolderModel(item));

                return Task.FromResult(folders.ToArray().AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<ModelObject>());
        }
    }
}
