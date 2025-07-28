using System;
using System.IO;

using dosymep.SimpleServices;

namespace RevitMepTotals.Services.Implements;
internal class DirectoryProvider : IDirectoryProvider {
    private readonly IOpenFolderDialogService _openFolderDialogService;


    public DirectoryProvider(IOpenFolderDialogService openFolderDialogService) {
        _openFolderDialogService = openFolderDialogService
            ?? throw new ArgumentNullException(nameof(openFolderDialogService));
    }


    public DirectoryInfo GetDirectory() {
        _openFolderDialogService.Multiselect = false;
        return _openFolderDialogService.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
            ? _openFolderDialogService.Folder
            : throw new OperationCanceledException();
    }
}
