using System;
using System.Linq;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.ViewModels;
using RevitRefreshLinks.Views;

namespace RevitRefreshLinks.Services {
    internal class RsOpenFileDialog : IOpenFileDialog {
        private readonly FilesExplorerViewModel _explorerViewModel;
        private readonly FilesExplorerWindow _explorerWindow;

        public RsOpenFileDialog(FilesExplorerViewModel explorerViewModel, FilesExplorerWindow explorerWindow) {
            _explorerViewModel = explorerViewModel ?? throw new ArgumentNullException(nameof(explorerViewModel));
            _explorerWindow = explorerWindow ?? throw new ArgumentNullException(nameof(explorerWindow));
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

        public bool ShowDialog() {
            return _explorerWindow.ShowDialog() ?? false;
        }
    }
}
