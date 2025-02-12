using System;
using System.Linq;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.ViewModels;
using RevitRefreshLinks.Views;

namespace RevitRefreshLinks.Services {
    internal class RsOpenFolderDialog : IOpenFolderDialog {
        private readonly DirectoriesExplorerViewModel _explorerViewModel;
        private readonly DirectoriesExplorerWindow _explorerWindow;

        public RsOpenFolderDialog(
            DirectoriesExplorerViewModel explorerViewModel,
            DirectoriesExplorerWindow explorerWindow) {

            _explorerViewModel = explorerViewModel ?? throw new ArgumentNullException(nameof(explorerViewModel));
            _explorerWindow = explorerWindow ?? throw new ArgumentNullException(nameof(explorerWindow));
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
            return _explorerWindow.ShowDialog() ?? false;
        }
    }
}
