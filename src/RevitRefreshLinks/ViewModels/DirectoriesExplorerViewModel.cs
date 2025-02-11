using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels {
    internal class DirectoriesExplorerViewModel : BaseViewModel {
        private readonly IFileSystem _fileSystem;
        private readonly ILocalizationService _localizationService;
        private readonly IExplorerSettings _settings;
        private DirectoryViewModel _activeDirectory;
        private DirectoryViewModel _selectedDirectory;
        private DirectoryViewModel _rootDirectory;
        private ObservableCollection<DirectoryViewModel> _selectedDirectories;
        private string _title;
        private string _errorText;
        private bool _multiSelect;

        public DirectoriesExplorerViewModel(
            IFileSystem fileSystem,
            ILocalizationService localizationService,
            IExplorerSettings settings) {

            _fileSystem = fileSystem
                ?? throw new System.ArgumentNullException(nameof(fileSystem));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));
            _settings = settings ??
                throw new System.ArgumentNullException(nameof(settings));

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
            set {
                RaiseAndSetIfChanged(ref _activeDirectory, value);
            }
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
            private set => RaiseAndSetIfChanged(ref _title, value);
        }

        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool MultiSelect {
            get => _multiSelect;
            private set => RaiseAndSetIfChanged(ref _multiSelect, value);
        }

        private async Task LoadViewAsync() {
            if(!string.IsNullOrWhiteSpace(_settings.Title)) {
                Title = _settings.Title;
            } else {
                Title = _localizationService.GetLocalizedString("DirectoriesExplorer.Title");
            }

            RootDirectory = new DirectoryViewModel(await _fileSystem.GetRootDirectoryAsync());

            if(!string.IsNullOrWhiteSpace(_settings.InitialDirectory)) {
                try {
                    ActiveDirectory = new DirectoryViewModel(
                        await _fileSystem.GetDirectoryAsync(_settings.InitialDirectory));
                } catch(InvalidOperationException) {
                    ActiveDirectory = RootDirectory;
                }
            } else {
                ActiveDirectory = RootDirectory;
            }
            await ActiveDirectory.LoadContentAsync();
            MultiSelect = _settings.MultiSelect;
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

        }

        private bool CanAcceptView() {
            if(MultiSelect && (SelectedDirectories is null || SelectedDirectories.Count == 0)) {
                ErrorText = _localizationService.GetLocalizedString("TODO");
                return false;
            }
            if(!MultiSelect && SelectedDirectory is null) {
                ErrorText = _localizationService.GetLocalizedString("TODO");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
