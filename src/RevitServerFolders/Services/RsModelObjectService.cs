using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using dosymep.Revit.ServerClient;
using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitServerFolders.Models;
using RevitServerFolders.Models.Rs;
using RevitServerFolders.ViewModels.Rs;
using RevitServerFolders.Views.Rs;

namespace RevitServerFolders.Services;
internal sealed class RsModelObjectService : IModelObjectService {
    private readonly IResolutionRoot _resolutionRoot;
    private readonly MainViewModel _mainViewModel;
    private readonly IReadOnlyCollection<IServerClient> _serverClients;
    private readonly ILocalizationService _localization;

    public RsModelObjectService(IResolutionRoot resolutionRoot,
        MainViewModel mainViewModel,
        IReadOnlyCollection<IServerClient> serverClients,
        ILocalizationService localization) {
        _resolutionRoot = resolutionRoot;
        _mainViewModel = mainViewModel;
        _serverClients = serverClients;
        _localization = localization;
    }


    public bool IsAttached => false;

    public bool AllowAttach => false;

    public DependencyObject AssociatedObject => default;


    public Task<ModelObject> SelectModelObjectDialog() {
        return SelectModelObjectDialog(null);
    }

    public Task<ModelObject> SelectModelObjectDialog(string rootFolder) {
        _mainViewModel.RemoveCancellation();
        var window = _resolutionRoot.Get<MainWindow>();
        window.DataContext = _mainViewModel;
        if(window.ShowDialog() == true) {
            return Task.FromResult(_mainViewModel.SelectedItem.GetModelObject());
        }
        _mainViewModel.CancelCommands();

        throw new OperationCanceledException();
    }

    public async Task<ModelObject> GetFromString(string folderName) {
        var uri = new Uri(folderName.Replace(@"\", @"/"));
        var serverClient = _serverClients.FirstOrDefault(
            item => item.ServerName.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
        if(serverClient == null) {
            throw new InvalidOperationException(
                _localization.GetLocalizedString("Exceptions.ServerNotFound", uri.Host));
        }

        string parent = Path.GetDirectoryName(uri.LocalPath);
        string currentFolder = Path.GetFileName(uri.LocalPath);

        var folderContents = string.IsNullOrEmpty(parent)
            ? await serverClient.GetRootFolderContentsAsync()
            : await serverClient.GetFolderContentsAsync(parent);

        var folderData = folderContents.Folders
            .FirstOrDefault(item => item.Name.Equals(currentFolder));

        return folderData == null
            ? throw new InvalidOperationException(
                _localization.GetLocalizedString("Exceptions.FolderNotFound", uri.LocalPath.Trim('\\').Trim('/')))
            : (ModelObject) new RsFolderModel(folderData, folderContents, serverClient);
    }

    public void Detach() {
        // nothing
    }

    public void Attach(DependencyObject dependencyObject) {
        // nothing
    }
}
