using System;
using System.IO;

using dosymep.SimpleServices;

namespace RevitMepTotals.Services.Implements {
    internal class DirectoryProvider : IDirectoryProvider {
        private readonly IOpenFolderDialogService _openFolderDialogService;


        public DirectoryProvider(IOpenFolderDialogService openFolderDialogService) {
            _openFolderDialogService = openFolderDialogService
                ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        }


        public DirectoryInfo GetDirectory() {
            _openFolderDialogService.Multiselect = false;
            if(_openFolderDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) {
                return _openFolderDialogService.Folder;
            } else {
                throw new OperationCanceledException();
            }
        }
    }
}
