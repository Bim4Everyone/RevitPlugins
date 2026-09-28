using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

/// <summary>
/// Модуль типовой аннотации. Семейство и типоразмер выбираются по именам:
/// из проекта (IsFromFolder = false) или из папки с семействами (IsFromFolder = true).
/// В режиме папки семейство загружается в проект с заменой при создании компонента.
/// </summary>
internal class TypicalAnnotationVM : SheetComponentVM {
    /// <summary>
    /// Категория семейств, с которыми работает модуль
    /// </summary>
    private const BuiltInCategory _builtInCategory = BuiltInCategory.OST_GenericAnnotation;

    private readonly FamilyLibraryService _familyLibraryService;
    private readonly IOpenFolderDialogService _openFolderDialogService;

    private bool _isFromFolder;
    private string _familyFolderPath = string.Empty;

    private List<string> _familyNames = [];
    private string _selectedFamilyName;
    private List<string> _typeNames = [];
    private string _selectedTypeName;

    private FiltrationComboBoxFilterListVM _familyNameFilter;
    private FiltrationComboBoxFilterListVM _typeNameFilter;

    private string _selectionError = string.Empty;

    // Флаг обновления списков: пока списки пересобираются, команды выбора из UI не выполняются
    private bool _isUpdating;

    // Имена из конфигурации, которые не удалось найти. Хранятся для текста ошибки и для повторного экспорта
    private string _missingFamilyName;
    private string _missingTypeName;

    // Ошибки получения списков
    private string _familyListError;
    private string _typeListError;

    public TypicalAnnotationVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetVM sheetVM,
        ILocalizationService localizationService,
        FamilyLibraryService familyLibraryService,
        IOpenFolderDialogService openFolderDialogService)
        : base(repository, stringParamSetService, sheetSetParams, sheetVM, localizationService) {

        _familyLibraryService = familyLibraryService;
        _openFolderDialogService = openFolderDialogService;

        SelectFolderCommand = RelayCommand.Create(SelectFolder, CanSelectFolder);
        ChangeFamilySourceCommand = RelayCommand.Create(ChangeFamilySource);
        SelectFamilyCommand = RelayCommand.Create(SelectFamily);
        SelectTypeCommand = RelayCommand.Create(SelectType);
    }

    public ICommand SelectFolderCommand { get; }

    /// <summary>
    /// Команда смены источника семейства, вызывается тумблером
    /// </summary>
    public ICommand ChangeFamilySourceCommand { get; }

    /// <summary>
    /// Команда выбора семейства, вызывается выпадающим списком семейств
    /// </summary>
    public ICommand SelectFamilyCommand { get; }

    /// <summary>
    /// Команда выбора типоразмера, вызывается выпадающим списком типоразмеров
    /// </summary>
    public ICommand SelectTypeCommand { get; }

    /// <summary>
    /// Источник семейства: false - проект, true - папка с семействами
    /// </summary>
    public bool IsFromFolder {
        get => _isFromFolder;
        set => RaiseAndSetIfChanged(ref _isFromFolder, value);
    }

    /// <summary>
    /// Путь до папки с семействами
    /// </summary>
    public string FamilyFolderPath {
        get => _familyFolderPath;
        set => RaiseAndSetIfChanged(ref _familyFolderPath, value);
    }

    /// <summary>
    /// Подсказка кнопки выбора папки - текущий путь до папки
    /// </summary>
    public string FolderButtonToolTip => string.IsNullOrWhiteSpace(FamilyFolderPath)
        ? LocalizationService.GetLocalizedString("Validation.FamilyFolderIsEmpty")
        : FamilyFolderPath;

    public List<string> FamilyNames {
        get => _familyNames;
        private set => RaiseAndSetIfChanged(ref _familyNames, value);
    }

    public string SelectedFamilyName {
        get => _selectedFamilyName;
        set => RaiseAndSetIfChanged(ref _selectedFamilyName, value);
    }

    public List<string> TypeNames {
        get => _typeNames;
        private set => RaiseAndSetIfChanged(ref _typeNames, value);
    }

    public string SelectedTypeName {
        get => _selectedTypeName;
        set => RaiseAndSetIfChanged(ref _selectedTypeName, value);
    }

    public FiltrationComboBoxFilterListVM FamilyNameFilter {
        get => _familyNameFilter;
        set => RaiseAndSetIfChanged(ref _familyNameFilter, value);
    }

    public FiltrationComboBoxFilterListVM TypeNameFilter {
        get => _typeNameFilter;
        set => RaiseAndSetIfChanged(ref _typeNameFilter, value);
    }

    /// <summary>
    /// Текст первой ошибки выбора семейства/типоразмера. Пустая строка - ошибок нет
    /// </summary>
    [ErrorTextIsEmpty]
    public string SelectionError {
        get => _selectionError;
        private set => RaiseAndSetIfChanged(ref _selectionError, value);
    }

    /// <summary>
    /// Имя семейства для сохранения в конфигурацию (включая имя, которое не удалось найти)
    /// </summary>
    public string FamilyNameForConfig => SelectedFamilyName ?? _missingFamilyName ?? string.Empty;

    /// <summary>
    /// Имя типоразмера для сохранения в конфигурацию (включая имя, которое не удалось найти)
    /// </summary>
    public string TypeNameForConfig => SelectedTypeName ?? _missingTypeName ?? string.Empty;


    /// <summary>
    /// Первичная инициализация из конфигурации: источник, папка, выбор семейства и типоразмера по именам
    /// </summary>
    public void InitializeSelection(bool isFromFolder, string familyFolderPath, string familyName, string typeName) {
        _isFromFolder = isFromFolder;
        _familyFolderPath = familyFolderPath ?? string.Empty;
        RaisePropertyChanged(nameof(IsFromFolder));
        RaisePropertyChanged(nameof(FamilyFolderPath));
        RaisePropertyChanged(nameof(FolderButtonToolTip));

        ResolveSelection(familyName, typeName);
    }

    /// <summary>
    /// Пересобирает списки семейств и типоразмеров из текущего источника и выбирает значения по именам
    /// </summary>
    private void ResolveSelection(string familyName, string typeName) {
        _isUpdating = true;
        try {
            // Семейства
            FamilyNames = IsFromFolder ? GetFolderFamilyNames() : GetProjectFamilyNames();

            // Если имя не задано, принимаем выбор, который сделал фильтр контрола,
            // когда после фильтрации остался единственный вариант
            string family = FamilyNames.Contains(familyName)
                ? familyName
                : string.IsNullOrEmpty(familyName) && FamilyNames.Contains(_selectedFamilyName)
                    ? _selectedFamilyName
                    : null;
            _missingFamilyName = family is null && !string.IsNullOrEmpty(familyName) ? familyName : null;
            _selectedFamilyName = family;

            // Типоразмеры
            TypeNames = family is null
                ? []
                : IsFromFolder ? GetFolderTypeNames(family) : GetProjectTypeNames(family);

            string type = TypeNames.Contains(typeName)
                ? typeName
                : string.IsNullOrEmpty(typeName) && TypeNames.Contains(_selectedTypeName)
                    ? _selectedTypeName
                    : null;
            _missingTypeName = type is null && !string.IsNullOrEmpty(typeName) ? typeName : null;
            _selectedTypeName = type;
        } finally {
            _isUpdating = false;
        }

        // Синхронизируем UI с итоговым выбором
        RaisePropertyChanged(nameof(SelectedFamilyName));
        RaisePropertyChanged(nameof(SelectedTypeName));
        UpdateSelectionError();
    }

    private List<string> GetProjectFamilyNames() {
        _familyListError = null;
        return Repository.GetFamilySymbols(_builtInCategory)
            .Select(s => s.FamilyName)
            .Distinct()
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private List<string> GetProjectTypeNames(string familyName) {
        _typeListError = null;
        return Repository.GetFamilySymbols(_builtInCategory)
            .Where(s => s.FamilyName.Equals(familyName))
            .Select(s => s.Name)
            .Distinct()
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private List<string> GetFolderFamilyNames() {
        _familyListError = null;

        if(string.IsNullOrWhiteSpace(FamilyFolderPath)) {
            _familyListError = GetText("Validation.FamilyFolderIsEmpty");
            return [];
        }

        List<string> familyNames;
        try {
            familyNames = Directory.Exists(FamilyFolderPath)
                ? _familyLibraryService.GetFamilyNames(FamilyFolderPath)
                : null;
        } catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException) {
            familyNames = null;
        }

        if(familyNames is null) {
            _familyListError = GetText("Validation.FamilyFolderNotFound", FamilyFolderPath);
            return [];
        }
        if(familyNames.Count == 0) {
            _familyListError = GetText("Validation.FamilyFolderHasNoFamilies");
        }
        return familyNames;
    }

    private List<string> GetFolderTypeNames(string familyName) {
        _typeListError = null;

        string familyPath = _familyLibraryService.GetFamilyPath(FamilyFolderPath, familyName);
        if(!_familyLibraryService.TryGetFamilyInfo(familyPath, out var familyInfo)) {
            _typeListError = GetText("Validation.FamilyTypesReadError", familyName);
            return [];
        }

        if(familyInfo.FamilyCategory == _builtInCategory) {
            return familyInfo.TypeNames
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        _typeListError = GetText("Validation.FamilyIsNotInCategory",
            familyName, Repository.GetCategoryName(_builtInCategory));
        return [];
    }

    /// <summary>
    /// Формирует первую ошибку выбора в порядке: папка -> семейство -> чтение семейства -> типоразмер
    /// </summary>
    private void UpdateSelectionError() {
        string error = null;

        // Ошибки папки учитываются только в режиме папки (в режиме проекта _familyListError всегда null)
        if(!string.IsNullOrEmpty(_familyListError)) {
            error = _familyListError;
        } else if(SelectedFamilyName is null) {
            error = string.IsNullOrEmpty(_missingFamilyName)
                ? GetText("Validation.FamilyIsNotSelected")
                : GetText(IsFromFolder ? "Validation.FamilyNotFoundInFolder" : "Validation.FamilyNotFoundInProject",
                    _missingFamilyName);
        } else if(!string.IsNullOrEmpty(_typeListError)) {
            error = _typeListError;
        } else if(SelectedTypeName is null) {
            error = string.IsNullOrEmpty(_missingTypeName)
                ? GetText("Validation.TypeIsNotSelected")
                : GetText("Validation.TypeNotFoundInFamily", _missingTypeName, SelectedFamilyName);
        }

        SelectionError = error ?? string.Empty;
    }

    private string GetText(string key, params object[] args) {
        string text = LocalizationService.GetLocalizedString(key) ?? key;
        return args.Length == 0 ? text : string.Format(text, args);
    }

    private void SelectFolder() {
        string initialDirectory = !string.IsNullOrWhiteSpace(FamilyFolderPath) && Directory.Exists(FamilyFolderPath)
            ? FamilyFolderPath
            : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if(!_openFolderDialogService.ShowDialog(initialDirectory)) {
            return;
        }

        FamilyFolderPath = _openFolderDialogService.Folder.FullName;
        RaisePropertyChanged(nameof(FolderButtonToolTip));
        // Папка сменилась - выбранные имена сохраняем и ищем их в новой папке
        ResolveSelection(FamilyNameForConfig, TypeNameForConfig);
    }

    /// <summary>
    /// Смена источника семейства тумблером: выбранные имена сохраняем и ищем их в новом источнике
    /// </summary>
    private void ChangeFamilySource() {
        ResolveSelection(FamilyNameForConfig, TypeNameForConfig);
    }

    /// <summary>
    /// Выбор семейства в списке. Типоразмер после смены семейства выбирается заново
    /// </summary>
    private void SelectFamily() {
        // Списки пересобираются кодом - выбор в UI в этот момент не является выбором пользователя
        if(_isUpdating) {
            return;
        }
        ResolveSelection(SelectedFamilyName, null);
    }

    /// <summary>
    /// Выбор типоразмера в списке
    /// </summary>
    private void SelectType() {
        if(_isUpdating) {
            return;
        }
        _missingTypeName = null;
        UpdateSelectionError();
    }

    private bool CanSelectFolder() {
        return IsFromFolder;
    }


    public override void Process(bool processDependent = false) {
        var annotationType = GetAnnotationType();
        // Если семейство/типоразмер получить не удалось - компонент пропускается
        if(annotationType is null) {
            return;
        }
        var instance = Place(annotationType);
        SetCustomParams(instance);
    }

    /// <summary>
    /// Возвращает типоразмер для размещения. В режиме папки предварительно загружает семейство в проект с заменой.
    /// Должен вызываться внутри открытой транзакции.
    /// </summary>
    private FamilySymbol GetAnnotationType() {
        if(string.IsNullOrEmpty(SelectedFamilyName) || string.IsNullOrEmpty(SelectedTypeName)) {
            return null;
        }

        if(IsFromFolder) {
            string familyPath = _familyLibraryService.GetFamilyPath(FamilyFolderPath, SelectedFamilyName);
            if(_familyLibraryService.LoadFamily(familyPath) is null) {
                return null;
            }
        }

        var annotationType = Repository.GetFamilySymbol(_builtInCategory, SelectedFamilyName, SelectedTypeName);
        if(annotationType == null
           || annotationType.IsActive) {
            return annotationType;
        }

        annotationType.Activate();
        Repository.Document.Regenerate();
        return annotationType;
    }

    private FamilyInstance Place(FamilySymbol annotationType) {
        var position = new XYZ(
            UnitUtilsHelper.ConvertToInternalValue(-100),
            UnitUtilsHelper.ConvertToInternalValue(250),
            0);
        return Repository.Document.Create.NewFamilyInstance(position, annotationType, Sheet.SheetInstance);
    }
}
