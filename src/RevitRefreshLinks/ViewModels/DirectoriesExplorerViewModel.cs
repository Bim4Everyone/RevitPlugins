using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels {
    internal class DirectoriesExplorerViewModel : BaseViewModel {
        private readonly IFileSystem _fileSystem;
        private readonly ILocalizationService _localizationService;
        private DirectoryViewModel _activeDirectory;
        private DirectoryViewModel _selectedDirectory;
        private DirectoryViewModel _rootDirectory;
        private ObservableCollection<DirectoryViewModel> _selectedDirectories;
        private string _title;
        private string _errorText;
        private bool _multiSelect;
        private string _initialDirectory;

        public DirectoriesExplorerViewModel(
            IFileSystem fileSystem,
            ILocalizationService localizationService) {

            _fileSystem = fileSystem
                ?? throw new System.ArgumentNullException(nameof(fileSystem));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));

            LoadViewCommand = RelayCommand.CreateAsync(LoadViewAsync);
            OpenFolderCommand = RelayCommand.CreateAsync<DirectoryViewModel>(OpenFolderAsync, CanOpenFolder);
            OpenParentFolderCommand = RelayCommand.CreateAsync(OpenParentFolderAsync, CanOpenParentFolder);
            OpenRootFolderCommand = RelayCommand.CreateAsync(OpenRootFolderAsync);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }


        public ICommand LoadViewCommand { get; }

        public ICommand OpenFolderCommand { get; }

        public ICommand OpenParentFolderCommand { get; }

        public ICommand OpenRootFolderCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public DirectoryViewModel ActiveDirectory {
            get => _activeDirectory;
            set => RaiseAndSetIfChanged(ref _activeDirectory, value);
        }

        public DirectoryViewModel RootDirectory {
            get => _rootDirectory;
            private set => RaiseAndSetIfChanged(ref _rootDirectory, value);
        }

        public DirectoryViewModel SelectedDirectory {
            get => _selectedDirectory;
            set => RaiseAndSetIfChanged(ref _selectedDirectory, value);
        }

        public ObservableCollection<DirectoryViewModel> SelectedDirectories {
            get => _selectedDirectories;
            set => RaiseAndSetIfChanged(ref _selectedDirectories, value);
        }

        public string Title {
            get => _title;
            set => RaiseAndSetIfChanged(ref _title, value);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool MultiSelect {
            get => _multiSelect;
            set => RaiseAndSetIfChanged(ref _multiSelect, value);
        }

        public string InitialDirectory {
            get => _initialDirectory;
            set => RaiseAndSetIfChanged(ref _initialDirectory, value);
        }

        private async Task LoadViewAsync() {
            RootDirectory = new DirectoryViewModel(await _fileSystem.GetRootDirectoryAsync());

            if(!string.IsNullOrWhiteSpace(InitialDirectory)) {
                try {
                    ActiveDirectory = new DirectoryViewModel(
                        await _fileSystem.GetDirectoryAsync(InitialDirectory));
                } catch(InvalidOperationException) {
                    ActiveDirectory = RootDirectory;
                }
            } else {
                ActiveDirectory = RootDirectory;
            }
            await ActiveDirectory.LoadContentAsync();
        }

        private async Task OpenFolderAsync(DirectoryViewModel folder) {
            ActiveDirectory = folder;
            await ActiveDirectory.LoadContentAsync();
        }

        private bool CanOpenFolder(DirectoryViewModel folder) {
            return folder != null;
        }

        private async Task OpenParentFolderAsync() {
            ActiveDirectory = await ActiveDirectory.GetParent();
            await ActiveDirectory.LoadContentAsync();
        }

        private bool CanOpenParentFolder() {
            return ActiveDirectory != null;
        }

        private async Task OpenRootFolderAsync() {
            ActiveDirectory = RootDirectory;
            await ActiveDirectory.LoadContentAsync();
        }

        private void AcceptView() {
            // TODO
        }

        private bool CanAcceptView() {
            if(MultiSelect && (SelectedDirectories is null || SelectedDirectories.Count == 0)) {
                ErrorText = _localizationService.GetLocalizedString("SelectLocalFoldersDialog.Title");
                return false;
            }
            if(!MultiSelect && SelectedDirectory is null) {
                ErrorText = _localizationService.GetLocalizedString("SelectLocalFoldersDialog.Title");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
