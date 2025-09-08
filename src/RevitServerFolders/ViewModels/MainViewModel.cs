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
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly IModelObjectService _objectService;
    private readonly object _locker = new();

    private string _errorText;
    private string _targetFolder;
    private string _sourceFolder;

    private ModelObjectViewModel _selectedObject;
    private ObservableCollection<ModelObjectViewModel> _modelObjects;

    private bool _isExportRooms;
    private bool _isExportRoomsVisible;
    private bool _clearTargetFolder;

    public MainViewModel(PluginConfig pluginConfig,
        IModelObjectService objectService,
        IOpenFolderDialogService openFolderDialogService,
        IProgressDialogFactory progressDialogFactory) {
        _pluginConfig = pluginConfig;
        _objectService = objectService;

        OpenFolderDialogService = openFolderDialogService;
        ProgressDialogFactory = progressDialogFactory;
        ModelObjects = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder);
        OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog);
        SourceFolderChangedCommand = RelayCommand.CreateAsync(SourceFolderChanged);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IAsyncCommand OpenFromFoldersCommand { get; }
    public ICommand OpenFolderDialogCommand { get; }
    public IAsyncCommand SourceFolderChangedCommand { get; }

    public IOpenFolderDialogService OpenFolderDialogService { get; }
    public IProgressDialogFactory ProgressDialogFactory { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string TargetFolder {
        get => _targetFolder;
        set => RaiseAndSetIfChanged(ref _targetFolder, value);
    }

    public bool ClearTargetFolder {
        get => _clearTargetFolder;
        set => RaiseAndSetIfChanged(ref _clearTargetFolder, value);
    }

    public string SourceFolder {
        get => _sourceFolder;
        set => RaiseAndSetIfChanged(ref _sourceFolder, value);
    }

    public ModelObjectViewModel SelectedObject {
        get => _selectedObject;
        set => RaiseAndSetIfChanged(ref _selectedObject, value);
    }

    public ObservableCollection<ModelObjectViewModel> ModelObjects {
        get => _modelObjects;
        set => RaiseAndSetIfChanged(ref _modelObjects, value);
    }

    public bool IsExportRooms {
        get => _isExportRooms;
        set => RaiseAndSetIfChanged(ref _isExportRooms, value);
    }

    public bool IsExportRoomsVisible {
        get => _isExportRoomsVisible;
        set => RaiseAndSetIfChanged(ref _isExportRoomsVisible, value);
    }

    protected virtual void LoadConfigImpl() { }
    protected virtual void SaveConfigImpl() { }
    protected virtual void AcceptViewImpl() { }

    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
        AcceptViewImpl();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(TargetFolder)) {
            ErrorText = "Выберите папку назначения.";
            return false;
        }

        if(!Directory.Exists(TargetFolder)) {
            ErrorText = "Выберите существующую папку назначения.";
            return false;
        }

        if(SourceFolder == null) {
            ErrorText = "Выберите папку источника.";
            return false;
        }

        if(ModelObjects.Count == 0) {
            ErrorText = "Выберите папку источника c моделями.";
            return false;
        }

        if(!ModelObjects
               .Any(item => !item.SkipObject)) {
            ErrorText = "Все модели помечены признаком пропустить.";
            return false;
        }

        if(OpenFromFoldersCommand.IsExecuting || SourceFolderChangedCommand.IsExecuting) {
            ErrorText = "Дождитесь завершения загрузки";
            return false;
        }

        string duplicateModelObject = ModelObjects
            .Where(item => !item.SkipObject)
            .GroupBy(item => item.Name)
            .Where(item => item.Count() > 1)
            .Select(item => item.Key)
            .FirstOrDefault();

        if(!string.IsNullOrEmpty(duplicateModelObject)) {
            ErrorText = $"Папка источника содержит дубликаты \"{duplicateModelObject}\".";
            return false;
        }

        ErrorText = null;
        return true;
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
                await AddModelObjects(await _objectService.GetFromString(SourceFolder));
            }
        } catch {
            // pass
        }
    }

    private void LoadConfig() {
        TargetFolder = _pluginConfig.TargetFolder;
        SourceFolder = _pluginConfig.SourceFolder;
        ClearTargetFolder = _pluginConfig.ClearTargetFolder;

        LoadConfigImpl();
    }

    private void SaveConfig() {
        _pluginConfig.TargetFolder = TargetFolder;
        _pluginConfig.SourceFolder = SourceFolder;
        _pluginConfig.ClearTargetFolder = ClearTargetFolder;
        _pluginConfig.SkippedObjects = ModelObjects
            .Where(item => item.SkipObject)
            .Select(item => item.FullName)
            .ToArray();


        SaveConfigImpl();
        _pluginConfig.SaveProjectConfig();
    }

    private async Task AddModelObjects(ModelObject modelObject) {
        if(modelObject != null) {
            var modelObjects = await modelObject.GetChildrenObjects();

            AddModelObjects(modelObjects, _pluginConfig.SkippedObjects);
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
