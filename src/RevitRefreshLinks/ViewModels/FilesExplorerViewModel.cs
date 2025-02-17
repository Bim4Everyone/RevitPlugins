using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
        private PathInfoViewModel _selectedItem;
        private DirectoryViewModel _activeDirectory;
        private DirectoryViewModel _rootDirectory;
        private FileViewModel _selectedFile;
        private ObservableCollection<FileViewModel> _selectedFiles = new ObservableCollection<FileViewModel>();
        private ObservableCollection<PathInfoViewModel> _selectedItems = new ObservableCollection<PathInfoViewModel>();
        private string _title;
        private string _errorText;
        private bool _multiSelect;
        private string _filter;
        private string _initialDirectory;

        public FilesExplorerViewModel(IFileSystem fileSystem, ILocalizationService localizationService) {
            _fileSystem = fileSystem
                ?? throw new System.ArgumentNullException(nameof(fileSystem));
            _localizationService = localizationService
                ?? throw new System.ArgumentNullException(nameof(localizationService));

            LoadViewCommand = RelayCommand.CreateAsync(LoadViewAsync);
            OpenFolderCommand = RelayCommand.CreateAsync<DirectoryViewModel>(OpenFolderAsync, CanOpenFolder);
            OpenParentFolderCommand = RelayCommand.CreateAsync(OpenParentFolderAsync, CanOpenParentFolder);
            OpenRootFolderCommand = RelayCommand.CreateAsync(OpenRootFolderAsync, CanOpenRootFolder);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            OpenNextFolderCommand = RelayCommand.CreateAsync(OpenNextFolderAsync, CanOpenNextFolder);
            OpenPreviousFolderCommand = RelayCommand.CreateAsync(OpenPreviousFolderAsync, CanOpenPreviousFolder);
            UpdateViewCommand = RelayCommand.CreateAsync(UpdateView, CanUpdateView);

            SelectedItems.CollectionChanged += SelectedItemsChanged;
        }


        public ICommand LoadViewCommand { get; }

        public ICommand OpenFolderCommand { get; }

        public ICommand OpenParentFolderCommand { get; }

        public ICommand OpenPreviousFolderCommand { get; }

        public ICommand OpenNextFolderCommand { get; }

        public ICommand OpenRootFolderCommand { get; }

        public ICommand AcceptViewCommand { get; }

        public ICommand UpdateViewCommand { get; }


        public DirectoryViewModel ActiveDirectory {
            get => _activeDirectory;
            set {
                RaiseAndSetIfChanged(ref _activeDirectory, value);
                SelectedFile = null;
                SelectedFiles.Clear();
                OnPropertyChanged(nameof(NextDirs));
                OnPropertyChanged(nameof(PreviousDirs));
            }
        }

        public DirectoryViewModel RootDirectory {
            get => _rootDirectory;
            private set => RaiseAndSetIfChanged(ref _rootDirectory, value);
        }

        public PathInfoViewModel SelectedItem {
            get => _selectedItem;
            set {
                RaiseAndSetIfChanged(ref _selectedItem, value);
                SelectedFile = value is FileViewModel file ? file : null;
            }
        }

        public FileViewModel SelectedFile {
            get => _selectedFile;
            set => RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public ObservableCollection<FileViewModel> SelectedFiles {
            get => _selectedFiles;
            set => RaiseAndSetIfChanged(ref _selectedFiles, value);
        }

        public ObservableCollection<PathInfoViewModel> SelectedItems {
            get => _selectedItems;
            set {
                RaiseAndSetIfChanged(ref _selectedItems, value);
                SelectedFiles.Clear();
                if(value != null) {
                    foreach(var item in value.OfType<FileViewModel>()) {
                        SelectedFiles.Add(item);
                    }
                }
            }
        }

        public Stack<DirectoryViewModel> PreviousDirs { get; } = new Stack<DirectoryViewModel>();

        public Stack<DirectoryViewModel> NextDirs { get; } = new Stack<DirectoryViewModel>();

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

        public string Filter {
            get => _filter;
            set => RaiseAndSetIfChanged(ref _filter, value);
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
            await ActiveDirectory.LoadContentAsync(true, Filter);
        }

        private async Task OpenFolderAsync(DirectoryViewModel folder) {
            if(!ActiveDirectory.Equals(folder)) {
                PreviousDirs.Push(ActiveDirectory);
                NextDirs.Clear();
                ActiveDirectory = folder;
                await ActiveDirectory.LoadContentAsync(true, Filter);
            }
        }

        private bool CanOpenFolder(DirectoryViewModel folder) {
            return folder != null;
        }

        private async Task OpenParentFolderAsync() {
            DirectoryViewModel parent = await ActiveDirectory.GetParent();
            await OpenFolderAsync(parent);
        }

        private bool CanOpenParentFolder() {
            return ActiveDirectory != null && !ActiveDirectory.Equals(RootDirectory);
        }

        private async Task OpenRootFolderAsync() {
            await OpenFolderAsync(RootDirectory);
        }

        private bool CanOpenRootFolder() {
            return RootDirectory != null && !RootDirectory.Equals(ActiveDirectory);
        }

        private async Task UpdateView() {
            await ActiveDirectory.LoadContentAsync(true, Filter);
        }

        private bool CanUpdateView() {
            return ActiveDirectory != null;
        }

        private void AcceptView() {
            // nothing
        }

        private bool CanAcceptView() {
            if(MultiSelect && (SelectedFiles is null || SelectedFiles.Count == 0)) {
                ErrorText = _localizationService.GetLocalizedString("SelectLinksFromFolderDialog.Title");
                return false;
            }
            if(!MultiSelect && SelectedFile is null) {
                ErrorText = _localizationService.GetLocalizedString("SelectLinksFromFolderDialog.Title");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private async Task OpenPreviousFolderAsync() {
            if(PreviousDirs.Count > 0) {
                NextDirs.Push(ActiveDirectory);
                ActiveDirectory = PreviousDirs.Pop();
                await ActiveDirectory.LoadContentAsync(true, Filter);
            }
        }

        private bool CanOpenPreviousFolder() {
            return PreviousDirs.Count > 0;
        }

        private async Task OpenNextFolderAsync() {
            if(NextDirs.Count > 0) {
                PreviousDirs.Push(ActiveDirectory);
                ActiveDirectory = NextDirs.Pop();
                await ActiveDirectory.LoadContentAsync(true, Filter);
            }
        }

        private bool CanOpenNextFolder() {
            return NextDirs.Count > 0;
        }

        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Reset) {
                SelectedFiles.Clear();
            }
            var removedFiles = e.OldItems?.OfType<FileViewModel>().ToArray() ?? Array.Empty<FileViewModel>();
            foreach(var item in removedFiles) {
                SelectedFiles.Remove(item);
            }
            var addedFiles = e.NewItems?.OfType<FileViewModel>().ToArray() ?? Array.Empty<FileViewModel>();
            foreach(var item in addedFiles) {
                if(!SelectedFiles.Contains(item)) {
                    SelectedFiles.Add(item);
                }
            }
        }
    }
}
