using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;
using RevitServerFolders.Services;

namespace RevitServerFolders.ViewModels;

internal class ExportSettingsViewModel<T> : BaseViewModel where T : ExportSettings {
    protected readonly T _settings;
    protected readonly ILocalizationService _localization;

    private string _targetFolder;
    private string _sourceFolder;
    private string _searchText;
    private string _excludedObjectPatternText;

    private ModelObjectViewModel _selectedObject;

    private bool _isExportRooms;
    private bool _isNwcExport;
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
        ObjectService = objectService;

        OpenFolderDialogService = openFolderDialogService
            ?? throw new ArgumentNullException(nameof(openFolderDialogService));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        ModelObjects = [];
        SelectedObjects = [];
        ModelObjectsView = new CollectionViewSource() { Source = ModelObjects };
        ModelObjectsView.Filter += ModelObjectsFilterHandler;
        ExcludedObjectPatterns = [
            .. (_settings.ExcludedObjectPatterns ?? [])
            .Select(item => new ExcludedObjectPatternViewModel(item))
        ];
        TargetFolder = _settings.TargetFolder;
        SourceFolder = _settings.SourceFolder;
        ClearTargetFolder = _settings.ClearTargetFolder;
        OpenTargetWhenFinish = _settings.OpenTargetWhenFinish;
        IsSelected = _settings.IsSelected;
        TargetFromLabel = _localization.GetLocalizedString("MainWindow.TargetsFrom");
        TargetToLabel = _localization.GetLocalizedString("MainWindow.TargetsTo");

        OpenFromFoldersCommand = RelayCommand.CreateAsync(OpenFromFolder);
        OpenFolderDialogCommand = RelayCommand.Create(OpenFolderDialog);
        SourceFolderChangedCommand = RelayCommand.CreateAsync(SourceFolderChanged);
        ToggleSkipObjectCommand = RelayCommand.Create<ModelObjectViewModel>(ToggleSkipObject);
        AddExcludedObjectPatternCommand = RelayCommand.Create(
            AddExcludedObjectPattern,
            CanAddExcludedObjectPattern);
        RemoveExcludedObjectPatternCommand = RelayCommand.Create<ExcludedObjectPatternViewModel>(
            RemoveExcludedObjectPattern,
            CanRemoveExcludedObjectPattern);

        PropertyChanged += OnSourceFolderChanged;
        PropertyChanged += OnSearchTextChanged;
    }

    public IModelObjectService ObjectService { get; }

    public IAsyncCommand OpenFromFoldersCommand { get; }

    public ICommand OpenFolderDialogCommand { get; }

    public IAsyncCommand<object> SourceFolderChangedCommand { get; }

    public ICommand ToggleSkipObjectCommand { get; }

    public ICommand AddExcludedObjectPatternCommand { get; }

    public ICommand RemoveExcludedObjectPatternCommand { get; }

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

    /// <summary>
    /// Модели, выделенные в таблице
    /// </summary>
    public ObservableCollection<ModelObjectViewModel> SelectedObjects { get; }

    /// <summary>
    /// Отфильтрованное представление <see cref="ModelObjects"/>: поиск и скрытие по подстрокам
    /// </summary>
    public CollectionViewSource ModelObjectsView { get; }

    public ObservableCollection<ExcludedObjectPatternViewModel> ExcludedObjectPatterns { get; }

    /// <summary>
    /// Подстрока для поиска по списку моделей
    /// </summary>
    public string SearchText {
        get => _searchText;
        set => RaiseAndSetIfChanged(ref _searchText, value);
    }

    /// <summary>
    /// Подстрока для скрытия файлов, из которой создается карточка исключения
    /// </summary>
    public string ExcludedObjectPatternText {
        get => _excludedObjectPatternText;
        set => RaiseAndSetIfChanged(ref _excludedObjectPatternText, value);
    }

    public bool SkipAll {
        get => _skipAll;
        set {
            if(_skipAll != value) {
                RaiseAndSetIfChanged(ref _skipAll, value);
                foreach(var item in GetVisibleObjects()) {
                    item.SkipObject = value;
                }
            }
        }
    }

    public bool IsExportRooms {
        get => _isExportRooms;
        set => RaiseAndSetIfChanged(ref _isExportRooms, value);
    }

    public bool IsNwcExport {
        get => _isNwcExport;
        set => RaiseAndSetIfChanged(ref _isNwcExport, value);
    }

    public virtual T GetSettings() {
        _settings.Index = Index;
        _settings.TargetFolder = TargetFolder;
        _settings.SourceFolder = SourceFolder;
        _settings.ClearTargetFolder = ClearTargetFolder;
        _settings.OpenTargetWhenFinish = OpenTargetWhenFinish;
        _settings.IsSelected = IsSelected;
        _settings.SkippedObjects = ModelObjects
            .Where(item => item.SkipObject)
            .Select(item => item.FullName)
            .ToArray();
        _settings.ExcludedObjectPatterns = [.. ExcludedObjectPatterns.Select(item => item.GetSettings())];
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

        if(ModelObjects.All(item => item.SkipObject)) {
            return _localization.GetLocalizedString("MainWindow.Validation.AllModelsSkipped");
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
        var modelObject = await ObjectService.SelectModelObjectDialog(SourceFolder);
        SourceFolder = modelObject.FullName;
        await AddModelObjects(modelObject);
        CommandManager.InvalidateRequerySuggested();
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
                    await AddModelObjects(await ObjectService.GetFromString(SourceFolder));
                } else {
                    AddModelObjects([], []);
                }
            }
        } catch {
            // pass
        }
        CommandManager.InvalidateRequerySuggested();
    }

    private async void OnSourceFolderChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SourceFolder)) {
            await Task.Delay(250);
            await SourceFolderChangedCommand.ExecuteAsync(default);
        }
    }

    private void OnSearchTextChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SearchText)) {
            ModelObjectsView.View?.Refresh();
        }
    }

    private void ModelObjectsFilterHandler(object sender, FilterEventArgs e) {
        if(e.Item is ModelObjectViewModel item) {
            e.Accepted = !IsExcluded(item) && MatchesSearch(item);
        }
    }

    private bool IsExcluded(ModelObjectViewModel item) {
        return ExcludedObjectPatterns.Any(pattern => Contains(item.FullName, pattern.Value));
    }

    private bool MatchesSearch(ModelObjectViewModel item) {
        string searchText = SearchText?.Trim();
        return string.IsNullOrEmpty(searchText)
               || Contains(item.Name, searchText)
               || Contains(item.FullName, searchText);
    }

    private bool Contains(string source, string value) {
        return !string.IsNullOrEmpty(source)
               && !string.IsNullOrEmpty(value)
               && source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    private ModelObjectViewModel[] GetVisibleObjects() {
        var view = ModelObjectsView.View;
        return view is null ? [] : [.. view.OfType<ModelObjectViewModel>()];
    }

    /// <summary>
    /// Переносит признак пропуска кликнутой модели на все выделенные строки таблицы
    /// </summary>
    private void ToggleSkipObject(ModelObjectViewModel item) {
        if(item is null
           || !SelectedObjects.Contains(item)) {
            return;
        }

        bool skipObject = item.SkipObject;
        foreach(var selectedObject in SelectedObjects.ToArray()) {
            selectedObject.SkipObject = skipObject;
        }
    }

    private void AddExcludedObjectPattern() {
        ExcludedObjectPatterns.Add(
            new ExcludedObjectPatternViewModel(
                new ExcludedObjectPattern(ExcludedObjectPatternText.Trim())));
        ExcludedObjectPatternText = null;
        ModelObjectsView.View?.Refresh();
    }

    private bool CanAddExcludedObjectPattern() {
        return !string.IsNullOrWhiteSpace(ExcludedObjectPatternText?.Trim());
    }

    private void RemoveExcludedObjectPattern(ExcludedObjectPatternViewModel pattern) {
        ExcludedObjectPatterns.Remove(pattern);
        ModelObjectsView.View?.Refresh();
    }

    private bool CanRemoveExcludedObjectPattern(ExcludedObjectPatternViewModel pattern) {
        return pattern is not null;
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
