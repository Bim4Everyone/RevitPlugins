using System;
using System.Linq;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.ViewModels;

namespace RevitRefreshLinks.Services;
internal class RsOpenFolderDialog : IOpenFolderDialog {
    private readonly DirectoriesExplorerViewModel _explorerViewModel;
    private readonly IDirectoriesExplorerWindowProvider _windowProvider;

    public RsOpenFolderDialog(
        DirectoriesExplorerViewModel explorerViewModel,
        IDirectoriesExplorerWindowProvider windowProvider) {

        _explorerViewModel = explorerViewModel ?? throw new ArgumentNullException(nameof(explorerViewModel));
        _windowProvider = windowProvider ?? throw new ArgumentNullException(nameof(windowProvider));
    }
    public IDirectoryModel Folder => _explorerViewModel.SelectedDirectory.DirectoryModel;

    public IDirectoryModel[] Folders => _explorerViewModel.SelectedDirectories
        .Select(v => v.DirectoryModel)
        .ToArray();

    public string Title {
        get => _explorerViewModel.Title;
        set => _explorerViewModel.Title = value;
    }

    public string InitialDirectory {
        get => _explorerViewModel.InitialDirectory;
        set => _explorerViewModel.InitialDirectory = value;
    }

    public bool MultiSelect {
        get => _explorerViewModel.MultiSelect;
        set => _explorerViewModel.MultiSelect = value;
    }

    public bool ShowDialog() {
        return _windowProvider.GetDirectoriesExplorerWindow().ShowDialog() ?? false;
    }
}
