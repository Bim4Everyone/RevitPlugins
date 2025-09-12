using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;

internal class ExportSettingsViewModel<T> : BaseViewModel where T : ExportSettings {
    private readonly IModelObjectService _objectService;
    protected readonly T _settings;
    protected readonly ILocalizationService _localization;

    private string _targetFolder;
    private string _sourceFolder;

    private ModelObjectViewModel _selectedObject;

    private bool _isExportRooms;
    private bool _isExportRoomsVisible;
    private bool _clearTargetFolder;
    private bool _openTargetWhenFinish;
    private bool _skipAll;
    private bool _isSelected;
    private int _index;

    public ExportSettingsViewModel(T settings,
        IModelObjectService objectService,
        IOpenFolderDialogService openFolderDialogService,
        ILocalizationService localization) {
        _settings = settings;
        _objectService = objectService;

        OpenFolderDialogService = openFolderDialogService
            ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        ModelObjects = [];
        TargetFolder = _settings.TargetFolder;
        SourceFolder = _settings.SourceFolder;
        ClearTargetFolder = _settings.ClearTargetFolder;
        OpenTargetWhenFinish = _settings.OpenTargetWhenFinish;
        TargetFromLabel = _localization.GetLocalizedString("MainWindow.TargetsFrom");
        TargetToLabel = _localization.GetLocalizedString("MainWindow.TargetsTo");

        OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder);
        OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog);
        SourceFolderChangedCommand = RelayCommand.CreateAsync(SourceFolderChanged);
    }


    public AsyncRelayCommand OpenFromFoldersCommand { get; }

    public ICommand OpenFolderDialogCommand { get; }

    public AsyncRelayCommand SourceFolderChangedCommand { get; }

    public IOpenFolderDialogService OpenFolderDialogService { get; }

    public string TargetFromLabel { get; }

    public string TargetToLabel { get; }

    public int Index {
        get => _index;
        set => RaiseAndSetIfChanged(ref _index, value);
    }

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string TargetFolder {
        get => _targetFolder;
        set => RaiseAndSetIfChanged(ref _targetFolder, value);
    }

    public bool ClearTargetFolder {
        get => _clearTargetFolder;
        set => RaiseAndSetIfChanged(ref _clearTargetFolder, value);
    }

    public bool OpenTargetWhenFinish {
        get => _openTargetWhenFinish;
        set => RaiseAndSetIfChanged(ref _openTargetWhenFinish, value);
    }

    public string SourceFolder {
        get => _sourceFolder;
        set => RaiseAndSetIfChanged(ref _sourceFolder, value);
    }

    public ModelObjectViewModel SelectedObject {
        get => _selectedObject;
        set => RaiseAndSetIfChanged(ref _selectedObject, value);
    }

    public ObservableCollection<ModelObjectViewModel> ModelObjects { get; }

    public bool SkipAll {
        get => _skipAll;
        set {
            if(_skipAll != value) {
                RaiseAndSetIfChanged(ref _skipAll, value);
                foreach(var item in ModelObjects) {
                    item.SkipObject = value;
                }
            }
        }
    }

    public bool IsExportRooms {
        get => _isExportRooms;
        set => RaiseAndSetIfChanged(ref _isExportRooms, value);
    }

    public bool IsExportRoomsVisible {
        get => _isExportRoomsVisible;
        set => RaiseAndSetIfChanged(ref _isExportRoomsVisible, value);
    }

    public virtual T GetSettings() {
        _settings.TargetFolder = TargetFolder;
        _settings.SourceFolder = SourceFolder;
        _settings.ClearTargetFolder = ClearTargetFolder;
        _settings.OpenTargetWhenFinish = OpenTargetWhenFinish;
        _settings.SkippedObjects = ModelObjects
            .Where(item => item.SkipObject)
            .Select(item => item.FullName)
            .ToArray();
        return _settings;
    }

    public string GetErrorText() {
        if(string.IsNullOrEmpty(TargetFolder)) {
            return _localization.GetLocalizedString("MainWindow.Validation.SelectTargetFolder");
        }

        if(!Directory.Exists(TargetFolder)) {
            return _localization.GetLocalizedString("MainWindow.Validation.TargetFolderNotExist");
        }

        if(SourceFolder == null) {
            return _localization.GetLocalizedString("MainWindow.Validation.SelectSourceFolder");
        }

        if(ModelObjects.Count == 0) {
            return _localization.GetLocalizedString("MainWindow.Validation.SourceFolderEmpty");
        }

        if(!ModelObjects.Any(item => !item.SkipObject)) {
            return _localization.GetLocalizedString("MainWindow.Validation.AllModelsSkiped");
        }

        if(OpenFromFoldersCommand.IsExecuting || SourceFolderChangedCommand.IsExecuting) {
            return _localization.GetLocalizedString("MainWindow.Validation.Wait");
        }

        string duplicateModelObject = ModelObjects
            .Where(item => !item.SkipObject)
            .GroupBy(item => item.Name)
            .Where(item => item.Count() > 1)
            .Select(item => item.Key)
            .FirstOrDefault();

        if(!string.IsNullOrEmpty(duplicateModelObject)) {
            return _localization.GetLocalizedString("MainWindow.Validation.ModelsDuplicated", duplicateModelObject);
        }

        return string.Empty;
    }

    private async Task OpenFromFolder() {
        var modelObject = await _objectService.SelectModelObjectDialog(SourceFolder);
        SourceFolder = modelObject.FullName;
        await AddModelObjects(modelObject);
    }

    private void OpenFolderDialog() {
        if(OpenFolderDialogService.ShowDialog(TargetFolder)) {
            TargetFolder = OpenFolderDialogService.Folder.FullName;
        }
    }

    private async Task SourceFolderChanged() {
        try {
            if(!OpenFromFoldersCommand.IsExecuting) {
                if(!string.IsNullOrWhiteSpace(SourceFolder)) {
                    await AddModelObjects(await _objectService.GetFromString(SourceFolder));
                } else {
                    AddModelObjects([], []);
                }
            }
        } catch {
            // pass
        }
    }

    private async Task AddModelObjects(ModelObject modelObject) {
        if(modelObject != null) {
            var modelObjects = await modelObject.GetChildrenObjects();

            AddModelObjects(modelObjects, _settings.SkippedObjects);
        }
    }

    private void AddModelObjects(IEnumerable<ModelObject> modelObjects, string[] skippedObjects) {
        ModelObjects.Clear();

        modelObjects = modelObjects
            .OrderBy(item => item.Name);

        foreach(var child in modelObjects) {
            ModelObjects.Add(new ModelObjectViewModel(child));
        }

        foreach(var modelObjectViewModel in ModelObjects) {
            modelObjectViewModel.SkipObject = skippedObjects?
                .Contains(modelObjectViewModel.FullName, StringComparer.OrdinalIgnoreCase) == true;
        }
    }
}
