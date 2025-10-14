using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using dosymep.SimpleServices;

using RevitServerFolders.Models;
using RevitServerFolders.Models.FileSystem;

namespace RevitServerFolders.Services;
internal sealed class FileSystemModelObjectService : IModelObjectService {
    private readonly IOpenFolderDialogService _openFolderDialogService;

    public FileSystemModelObjectService(IOpenFolderDialogService openFolderDialogService) {
        _openFolderDialogService = openFolderDialogService;
    }

    public bool IsAttached => (_openFolderDialogService as IAttachableService)?.IsAttached ?? false;

    public bool AllowAttach => (_openFolderDialogService as IAttachableService)?.AllowAttach ?? false;

    public DependencyObject AssociatedObject => (_openFolderDialogService as IAttachableService)?.AssociatedObject ?? default;


    public Task<ModelObject> SelectModelObjectDialog() {
        return SelectModelObjectDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    public async Task<ModelObject> SelectModelObjectDialog(string rootFolder) {
        _openFolderDialogService.Multiselect = false;
        _openFolderDialogService.InitialDirectory = rootFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        return (await ShowDialog()).FirstOrDefault();
    }

    public Task<IEnumerable<ModelObject>> SelectModelObjectsDialog() {
        return SelectModelObjectsDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    public Task<IEnumerable<ModelObject>> SelectModelObjectsDialog(string rootFolder) {
        _openFolderDialogService.Multiselect = true;
        _openFolderDialogService.InitialDirectory = rootFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        return ShowDialog();
    }

    public Task<ModelObject> GetFromString(string folderName) {
        return Directory.Exists(folderName)
            ? Task.FromResult((ModelObject) new FileSystemFolderModel(new DirectoryInfo(folderName)))
            : throw new OperationCanceledException();
    }

    public void Detach() {
        (_openFolderDialogService as IAttachableService)?.Detach();
    }

    public void Attach(DependencyObject dependencyObject) {
        (_openFolderDialogService as IAttachableService)?.Attach(dependencyObject);
    }

    private Task<IEnumerable<ModelObject>> ShowDialog() {
        if(!_openFolderDialogService.ShowDialog()) {
            throw new OperationCanceledException();
        }

        IEnumerable<ModelObject> folders = _openFolderDialogService.Folders
            .Select(item => new FileSystemFolderModel(item));

        return Task.FromResult(folders);
    }
}
