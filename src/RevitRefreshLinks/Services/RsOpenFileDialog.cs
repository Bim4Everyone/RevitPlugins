using System;
using System.Linq;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.ViewModels;

namespace RevitRefreshLinks.Services;
internal class RsOpenFileDialog : IOpenFileDialog {
    private readonly FilesExplorerViewModel _explorerViewModel;
    private readonly IFilesExplorerWindowProvider _windowProvider;

    public RsOpenFileDialog(FilesExplorerViewModel explorerViewModel, IFilesExplorerWindowProvider windowProvider) {
        _explorerViewModel = explorerViewModel ?? throw new ArgumentNullException(nameof(explorerViewModel));
        _windowProvider = windowProvider ?? throw new ArgumentNullException(nameof(windowProvider));
    }


    public IFileModel File => _explorerViewModel.SelectedFile.FileModel;

    public IFileModel[] Files => _explorerViewModel.SelectedFiles
        .Select(f => f.FileModel)
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

    public string Filter {
        get => _explorerViewModel.Filter;
        set => _explorerViewModel.Filter = value;
    }


    public bool ShowDialog() {
        return _windowProvider.GetFilesExplorerWindow().ShowDialog() ?? false;
    }
}
