using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.ViewModels;
internal class DirectoriesExplorerViewModel : BaseViewModel {
    private readonly IFileSystem _fileSystem;
    private readonly ILocalizationService _localizationService;
    private readonly AsyncRelayCommand _loadViewCommand;
    private readonly AsyncRelayCommand<DirectoryViewModel> _openFolderCommand;
    private readonly AsyncRelayCommand _openParentFolderCommand;
    private readonly AsyncRelayCommand _openRootFolderCommand;
    private readonly AsyncRelayCommand _openNextFolderCommand;
    private readonly AsyncRelayCommand _openPreviousFolderCommand;
    private readonly AsyncRelayCommand _updateViewCommand;
    private DirectoryViewModel _activeDirectory;
    private DirectoryViewModel _selectedDirectory;
    private DirectoryViewModel _rootDirectory;
    private ObservableCollection<DirectoryViewModel> _selectedDirectories
        = [];
    private string _title;
    private string _errorText;
    private bool _multiSelect;
    private string _initialDirectory;
    private bool _anyCmdIsExecuting;

    public DirectoriesExplorerViewModel(
        IFileSystem fileSystem,
        ILocalizationService localizationService) {

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
            OnPropertyChanged(nameof(NextDirs));
            OnPropertyChanged(nameof(PreviousDirs));
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

    private async Task LoadViewAsync() {
        RootDirectory = new DirectoryViewModel(await _fileSystem.GetRootDirectoryAsync());

        if(ActiveDirectory is null) {
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
        await ActiveDirectory.LoadContentAsync(false);
    }

    private bool CanUpdateView() {
        return !AnyCmdIsExecuting && ActiveDirectory != null;
    }

    private void AcceptView() {
        InitialDirectory = ActiveDirectory.FullName;
        // nothing
    }

    private bool CanAcceptView() {
        if(AnyCmdIsExecuting) {
            ErrorText = _localizationService.GetLocalizedString("RsOpenFileWindow.ExecutingCmdCheck");
            return false;
        }
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
            await dir.LoadContentAsync(false);
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
