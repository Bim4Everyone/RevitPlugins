using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels;
internal class FilesExplorerViewModel : BaseViewModel {
    private readonly IFileSystem _fileSystem;
    private readonly ILocalizationService _localizationService;
    private readonly AsyncRelayCommand _loadViewCommand;
    private readonly AsyncRelayCommand<DirectoryViewModel> _openFolderCommand;
    private readonly AsyncRelayCommand _openParentFolderCommand;
    private readonly AsyncRelayCommand _openRootFolderCommand;
    private readonly AsyncRelayCommand _openNextFolderCommand;
    private readonly AsyncRelayCommand _openPreviousFolderCommand;
    private readonly AsyncRelayCommand _updateViewCommand;
    private PathInfoViewModel _selectedItem;
    private DirectoryViewModel _activeDirectory;
    private DirectoryViewModel _rootDirectory;
    private FileViewModel _selectedFile;
    private ObservableCollection<FileViewModel> _selectedFiles = [];
    private ObservableCollection<PathInfoViewModel> _selectedItems = [];
    private string _title;
    private string _errorText;
    private bool _multiSelect;
    private string _filter;
    private string _initialDirectory;
    private bool _anyCmdIsExecuting;

    public FilesExplorerViewModel(IFileSystem fileSystem, ILocalizationService localizationService) {
        _fileSystem = fileSystem
            ?? throw new System.ArgumentNullException(nameof(fileSystem));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));

        _loadViewCommand = RelayCommand.CreateAsync(LoadViewAsync);
        _openFolderCommand = RelayCommand.CreateAsync<DirectoryViewModel>(OpenFolderAsync, CanOpenFolder);
        _openParentFolderCommand = RelayCommand.CreateAsync(OpenParentFolderAsync, CanOpenParentFolder);
        _openRootFolderCommand = RelayCommand.CreateAsync(OpenRootFolderAsync, CanOpenRootFolder);
        _openNextFolderCommand = RelayCommand.CreateAsync(OpenNextFolderAsync, CanOpenNextFolder);
        _openPreviousFolderCommand = RelayCommand.CreateAsync(OpenPreviousFolderAsync, CanOpenPreviousFolder);
        _updateViewCommand = RelayCommand.CreateAsync(UpdateView, CanUpdateView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        _loadViewCommand.PropertyChanged += CmdIsExecutingChanged;
        _openFolderCommand.PropertyChanged += CmdIsExecutingChanged;
        _openParentFolderCommand.PropertyChanged += CmdIsExecutingChanged;
        _openRootFolderCommand.PropertyChanged += CmdIsExecutingChanged;
        _openNextFolderCommand.PropertyChanged += CmdIsExecutingChanged;
        _openPreviousFolderCommand.PropertyChanged += CmdIsExecutingChanged;
        _updateViewCommand.PropertyChanged += CmdIsExecutingChanged;

        SelectedItems.CollectionChanged += SelectedItemsChanged;
    }


    public IAsyncCommand LoadViewCommand => _loadViewCommand;

    public IAsyncCommand OpenFolderCommand => _openFolderCommand;

    public IAsyncCommand OpenParentFolderCommand => _openParentFolderCommand;

    public IAsyncCommand OpenPreviousFolderCommand => _openPreviousFolderCommand;

    public IAsyncCommand OpenNextFolderCommand => _openNextFolderCommand;

    public IAsyncCommand OpenRootFolderCommand => _openRootFolderCommand;

    public IAsyncCommand UpdateViewCommand => _updateViewCommand;

    public ICommand AcceptViewCommand { get; }


    public bool AnyCmdIsExecuting {
        get => _anyCmdIsExecuting;
        set => RaiseAndSetIfChanged(ref _anyCmdIsExecuting, value);
    }

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
        await LoadContentIfEmpty(ActiveDirectory);
    }

    private async Task OpenFolderAsync(DirectoryViewModel folder) {
        if(!ActiveDirectory.Equals(folder)) {
            PreviousDirs.Push(ActiveDirectory);
            NextDirs.Clear();
            ActiveDirectory = folder;
            await LoadContentIfEmpty(ActiveDirectory);
        }
    }

    private bool CanOpenFolder(DirectoryViewModel folder) {
        return !AnyCmdIsExecuting && folder != null;
    }

    private async Task OpenParentFolderAsync() {
        var parent = await ActiveDirectory.GetParent();
        await OpenFolderAsync(parent);
    }

    private bool CanOpenParentFolder() {
        return !AnyCmdIsExecuting && ActiveDirectory != null && !ActiveDirectory.Equals(RootDirectory);
    }

    private async Task OpenRootFolderAsync() {
        await OpenFolderAsync(RootDirectory);
    }

    private bool CanOpenRootFolder() {
        return !AnyCmdIsExecuting && RootDirectory != null;
    }

    private async Task UpdateView() {
        await ActiveDirectory.LoadContentAsync(true, Filter);
    }

    private bool CanUpdateView() {
        return !AnyCmdIsExecuting && ActiveDirectory != null;
    }

    private void AcceptView() {
        // nothing
    }

    private bool CanAcceptView() {
        if(AnyCmdIsExecuting) {
            ErrorText = _localizationService.GetLocalizedString("RsOpenFileWindow.ExecutingCmdCheck");
            return false;
        }
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
            await LoadContentIfEmpty(ActiveDirectory);
        }
    }

    private bool CanOpenPreviousFolder() {
        return !AnyCmdIsExecuting && PreviousDirs.Count > 0;
    }

    private async Task OpenNextFolderAsync() {
        if(NextDirs.Count > 0) {
            PreviousDirs.Push(ActiveDirectory);
            ActiveDirectory = NextDirs.Pop();
            await LoadContentIfEmpty(ActiveDirectory);
        }
    }

    private bool CanOpenNextFolder() {
        return !AnyCmdIsExecuting && NextDirs.Count > 0;
    }

    private async Task LoadContentIfEmpty(DirectoryViewModel dir) {
        if(dir?.Content?.Count == 0) {
            await dir.LoadContentAsync(true, Filter);
        }
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

    private void CmdIsExecutingChanged(object sender, PropertyChangedEventArgs e) {
        AnyCmdIsExecuting = LoadViewCommand.IsExecuting
            || OpenFolderCommand.IsExecuting
            || OpenParentFolderCommand.IsExecuting
            || OpenPreviousFolderCommand.IsExecuting
            || OpenNextFolderCommand.IsExecuting
            || OpenRootFolderCommand.IsExecuting
            || UpdateViewCommand.IsExecuting;
    }
}
