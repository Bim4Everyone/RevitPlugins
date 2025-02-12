using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels {
    internal class FilesExplorerViewModel : BaseViewModel {
        private readonly IFileSystem _fileSystem;
        private readonly ILocalizationService _localizationService;
        private DirectoryViewModel _activeDirectory;
        private DirectoryViewModel _rootDirectory;
        private FileViewModel _selectedFile;
        private ObservableCollection<FileViewModel> _selectedFiles;
        private string _title;
        private string _errorText;
        private bool _multiSelect;
        private string _initialDirectory;

        public FilesExplorerViewModel(IFileSystem fileSystem, ILocalizationService localizationService) {
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

        public FileViewModel SelectedFile {
            get => _selectedFile;
            set => RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public ObservableCollection<FileViewModel> SelectedFiles {
            get => _selectedFiles;
            set => RaiseAndSetIfChanged(ref _selectedFiles, value);
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
                } catch(System.InvalidOperationException) {
                    ActiveDirectory = RootDirectory;
                }
            } else {
                ActiveDirectory = RootDirectory;
            }
            await ActiveDirectory.LoadContentAsync(true);
        }

        private async Task OpenFolderAsync(DirectoryViewModel folder) {
            ActiveDirectory = folder;
            await ActiveDirectory.LoadContentAsync(true);
        }

        private bool CanOpenFolder(DirectoryViewModel folder) {
            return folder != null;
        }

        private async Task OpenParentFolderAsync() {
            ActiveDirectory = await ActiveDirectory.GetParent();
            await ActiveDirectory.LoadContentAsync(true);
        }

        private bool CanOpenParentFolder() {
            return ActiveDirectory != null;
        }

        private async Task OpenRootFolderAsync() {
            ActiveDirectory = RootDirectory;
            await ActiveDirectory.LoadContentAsync(true);
        }

        private void AcceptView() {
            // TODO
        }

        private bool CanAcceptView() {
            if(MultiSelect && (SelectedFiles is null || SelectedFiles.Count == 0)) {
                ErrorText = _localizationService.GetLocalizedString("TODO");
                return false;
            }
            if(!MultiSelect && SelectedFile is null) {
                ErrorText = _localizationService.GetLocalizedString("TODO");
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}
