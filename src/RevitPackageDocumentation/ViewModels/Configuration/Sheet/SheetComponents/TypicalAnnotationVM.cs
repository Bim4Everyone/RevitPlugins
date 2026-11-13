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

    // Отступ по вертикали, 100 мм, между рядом аннотаций и вышестоящим рядом (фрагменты, разрезы или рамка)
    private readonly double _annotationRowOffset = UnitUtilsHelper.ConvertToInternalValue(100);

    // Шаг по горизонтали, 120 мм, между левыми краями соседних аннотаций в ряду
    private readonly double _annotationStep = UnitUtilsHelper.ConvertToInternalValue(120);

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

    /// <summary>
    /// Команда выбора папки с семействами (доступна в режиме папки)
    /// </summary>
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

    /// <summary>
    /// Имена семейств из текущего источника
    /// </summary>
    public List<string> FamilyNames {
        get => _familyNames;
        private set => RaiseAndSetIfChanged(ref _familyNames, value);
    }

    public string SelectedFamilyName {
        get => _selectedFamilyName;
        set => RaiseAndSetIfChanged(ref _selectedFamilyName, value);
    }

    /// <summary>
    /// Имена типоразмеров выбранного семейства
    /// </summary>
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
            UpdateTypeNames(typeName);
        } finally {
            _isUpdating = false;
        }

        // Синхронизируем UI с итоговым выбором
        RaisePropertyChanged(nameof(SelectedFamilyName));
        RaisePropertyChanged(nameof(SelectedTypeName));
        UpdateSelectionError();
    }

    /// <summary>
    /// Пересобирает только список типоразмеров выбранного семейства и выбирает типоразмер по имени.
    /// Список семейств не меняется
    /// </summary>
    private void ResolveTypeSelection(string typeName) {
        _isUpdating = true;
        try {
            UpdateTypeNames(typeName);
        } finally {
            _isUpdating = false;
        }

        // Синхронизируем UI с итоговым выбором
        RaisePropertyChanged(nameof(SelectedTypeName));
        UpdateSelectionError();
    }

    /// <summary>
    /// Заполняет список типоразмеров выбранного семейства из текущего источника и выбирает типоразмер по имени.
    /// Вызывается при установленном флаге обновления списков
    /// </summary>
    private void UpdateTypeNames(string typeName) {
        TypeNames = _selectedFamilyName is null
            ? []
            : IsFromFolder ? GetFolderTypeNames(_selectedFamilyName) : GetProjectTypeNames(_selectedFamilyName);

        // Если имя не задано, принимаем выбор, который сделал фильтр контрола,
        // когда после фильтрации остался единственный вариант
        string type = TypeNames.Contains(typeName)
            ? typeName
            : string.IsNullOrEmpty(typeName) && TypeNames.Contains(_selectedTypeName)
                ? _selectedTypeName
                : null;
        _missingTypeName = type is null && !string.IsNullOrEmpty(typeName) ? typeName : null;
        _selectedTypeName = type;
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
    /// Выбор семейства в списке. Типоразмер после смены семейства выбирается заново.
    /// Список семейств при этом не пересобирается: выбор делается из уже заполненного списка
    /// (замена списка во время его изменения контролом приводила бы к повторному обновлению контрола)
    /// </summary>
    private void SelectFamily() {
        // Списки пересобираются кодом - выбор в UI в этот момент не является выбором пользователя
        if(_isUpdating) {
            return;
        }
        // Семейство выбрано пользователем или выбор сброшен фильтром - ненайденное имя из конфигурации больше не актуально
        _missingFamilyName = null;
        ResolveTypeSelection(null);
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

    /// <summary>
    /// Получает типоразмер (в режиме папки - с загрузкой семейства), размещает аннотацию и заполняет
    /// дополнительные параметры
    /// </summary>
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

    /// <summary>
    /// Размещает аннотацию под рамкой листа слева направо: ряд аннотаций располагается ниже фрагментов,
    /// если их нет - ниже разрезов, если нет и их - ниже рамки. Если такой типоразмер уже есть на листе,
    /// аннотация повторно не размещается.
    /// </summary>
    /// <returns>Размещенный экземпляр или null, если аннотация не размещалась</returns>
    private FamilyInstance Place(FamilySymbol annotationType) {
        var sheetInstance = Sheet.SheetInstance;
        if(sheetInstance is null || IsPlacedOnSheet(annotationType, sheetInstance)) {
            return null;
        }

        // Без рамки ряд под рамкой не построить - аннотация размещается в фиксированной точке слева от листа
        if(Repository.GetTitleBlock(sheetInstance) is not FamilyInstance titleBlock) {
            var position = new XYZ(
                UnitUtilsHelper.ConvertToInternalValue(-100),
                UnitUtilsHelper.ConvertToInternalValue(250),
                0);
            return Repository.Document.Create.NewFamilyInstance(position, annotationType, sheetInstance);
        }
        var titleBlockBB = titleBlock.get_BoundingBox(sheetInstance);
        double rowTopY = GetAnnotationRowTop(sheetInstance, titleBlockBB.Min.Y) - _annotationRowOffset;
        double? lastAnnotationLeftX = GetLastAnnotationLeft(sheetInstance, titleBlockBB.Min.Y);
        double rowLeftX = lastAnnotationLeftX.HasValue
            ? lastAnnotationLeftX.Value + _annotationStep
            : titleBlockBB.Min.X;

        // Точка вставки семейства может не совпадать с углом аннотации, поэтому аннотация размещается в начале
        // координат, а затем сдвигается так, чтобы левый верхний угол ее габарита оказался в нужной точке
        var instance = Repository.Document.Create.NewFamilyInstance(XYZ.Zero, annotationType, sheetInstance);
        Repository.Document.Regenerate();

        var instanceBB = instance.get_BoundingBox(sheetInstance);
        if(instanceBB != null) {
            var translation = new XYZ(rowLeftX - instanceBB.Min.X, rowTopY - instanceBB.Max.Y, 0);
            ElementTransformUtils.MoveElement(Repository.Document, instance.Id, translation);
        }
        return instance;
    }

    /// <summary>
    /// Проверяет, есть ли на листе экземпляр указанного типоразмера
    /// </summary>
    private bool IsPlacedOnSheet(FamilySymbol annotationType, ViewSheet sheetInstance) {
        return new FilteredElementCollector(Repository.Document, sheetInstance.Id)
            .OfClass(typeof(FamilyInstance))
            .OfType<FamilyInstance>()
            .Any(instance => instance.Symbol.Id == annotationType.Id);
    }

    /// <summary>
    /// Возвращает отметку, ниже которой располагается ряд аннотаций: низ самого нижнего фрагмента под рамкой,
    /// если фрагментов нет - низ самого нижнего разреза под рамкой, если нет и их - низ рамки
    /// </summary>
    private double GetAnnotationRowTop(ViewSheet sheetInstance, double titleBlockMinY) {
        var viewportsBelowTitleBlock = Repository.GetViewports(sheetInstance)
            .Where(viewport => viewport.GetBoxCenter().Y < titleBlockMinY)
            .Select(viewport => new {
                View = Repository.Document.GetElement(viewport.ViewId) as View,
                MinY = viewport.GetBoxOutline().MinimumPoint.Y
            })
            .Where(x => x.View != null)
            .ToList();

        var callouts = viewportsBelowTitleBlock.Where(x => x.View.IsCallout).ToList();
        if(callouts.Count > 0) {
            return callouts.Min(x => x.MinY);
        }

        var sections = viewportsBelowTitleBlock.Where(x => x.View is ViewSection && !x.View.IsCallout).ToList();
        if(sections.Count > 0) {
            return sections.Min(x => x.MinY);
        }
        return titleBlockMinY;
    }

    /// <summary>
    /// Возвращает левую границу самой правой аннотации под рамкой листа или null, если аннотаций под рамкой нет
    /// </summary>
    private double? GetLastAnnotationLeft(ViewSheet sheetInstance, double titleBlockMinY) {
        return new FilteredElementCollector(Repository.Document, sheetInstance.Id)
            .OfCategory(_builtInCategory)
            .WhereElementIsNotElementType()
            .Select(element => element.get_BoundingBox(sheetInstance))
            .Where(bb => bb != null && (bb.Min.Y + bb.Max.Y) / 2 < titleBlockMinY)
            .Select(bb => (double?) bb.Min.X)
            .Max();
    }
}
