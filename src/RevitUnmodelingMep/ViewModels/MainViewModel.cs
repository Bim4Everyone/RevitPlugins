using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using pyRevitLabs.Json;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;


internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IOpenFileDialogService _openFileDialogService;
    private readonly ISaveFileDialogService _saveFileDialogService;
    public IMessageBoxService MessageBoxService { get; }
    private readonly IConfigSerializer _configSerializer;
    private readonly ConsumableTemplateManager _consumableTemplateManager;
    private readonly CategoryOptionsProvider _categoryOptionsProvider;
    private readonly CategoryAssignmentBuilder _categoryAssignmentBuilder;

    private string _errorText;
    private string _saveProperty;
    private ObservableCollection<ConsumableTypeItem> _consumableTypes;
    private ObservableCollection<CategoryAssignmentItem> _categoryAssignments;
    private bool _onlyPlacedInProject = true;
    private int _lastConfigIndex;
    private readonly IReadOnlyList<CategoryOption> _categoryOptions;
    private bool _isViewLoaded;
    
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        IConfigSerializer configSerializer) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _openFileDialogService = openFileDialogService;
        _saveFileDialogService = saveFileDialogService;
        MessageBoxService = messageBoxService;
        _configSerializer = configSerializer;
        _categoryOptionsProvider = new CategoryOptionsProvider(_revitRepository.Document, _localizationService);
        _categoryAssignmentBuilder = new CategoryAssignmentBuilder(_revitRepository);
        _categoryOptions = _categoryOptionsProvider.CreateCategoryOptions();
        _consumableTemplateManager = new ConsumableTemplateManager(
            _revitRepository.VisSettingsStorage,
            ResolveCategoryOption);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddConsumableTypeCommand = RelayCommand.Create(AddConsumableType);
        RemoveConsumableTypeCommand = RelayCommand.Create<ConsumableTypeItem>(RemoveConsumableType, CanRemoveConsumableType);
        ImportConfigsCommand = RelayCommand.Create(ImportConfigs);
        ExportConfigsCommand = RelayCommand.Create(ExportConfigs);
        RefreshConfigsCommand = RelayCommand.Create(RefreshConfigs);
        ResetConfigsCommand = RelayCommand.Create<Window>(ResetConfigs);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>();
        CategoryAssignments = new ObservableCollection<CategoryAssignmentItem>();

        Hint = new HintPanelViewModel(_localizationService, ResolveCategoryId);
    }


    public ICommand LoadViewCommand { get; }
    

    public ICommand AcceptViewCommand { get; }

    public ICommand AddConsumableTypeCommand { get; }

    public ICommand RemoveConsumableTypeCommand { get; }

    public ICommand ImportConfigsCommand { get; }

    public ICommand ExportConfigsCommand { get; }

    public ICommand RefreshConfigsCommand { get; }

    public ICommand ResetConfigsCommand { get; }

    public HintPanelViewModel Hint { get; }
    public IOpenFileDialogService OpenFileDialogService => _openFileDialogService;
    public ISaveFileDialogService SaveFileDialogService => _saveFileDialogService;


    /// <summary>
    /// Хранит текст ошибки валидации, который показывается в нижней части окна.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    /// <summary>
    /// Хранит имя параметра, в который сохраняется результат работы настроек.
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    /// <summary>
    /// Хранит редактируемый список расходников и переподключает к нему менеджер шаблонов при замене коллекции.
    /// </summary>
    public ObservableCollection<ConsumableTypeItem> ConsumableTypes {
        get => _consumableTypes;
        set {
            RaiseAndSetIfChanged(ref _consumableTypes, value);
            _consumableTemplateManager?.Attach(_consumableTypes);
        }
    }

    /// <summary>
    /// Хранит построенные группы назначений расходников на типы элементов Revit.
    /// </summary>
    public ObservableCollection<CategoryAssignmentItem> CategoryAssignments {
        get => _categoryAssignments;
        set => RaiseAndSetIfChanged(ref _categoryAssignments, value);
    }

    public IReadOnlyList<CategoryOption> CategoryOptions => _categoryOptions;
    public bool IsViewLoaded => _isViewLoaded;

    /// <summary>
    /// Управляет режимом отображения только размещенных в проекте типов и пересобирает списки после изменения.
    /// </summary>
    public bool OnlyPlacedInProject {
        get => _onlyPlacedInProject;
        set {
            if(_onlyPlacedInProject == value) {
                return;
            }

            RaiseAndSetIfChanged(ref _onlyPlacedInProject, value);

            if(_isViewLoaded) {
                UpdateTypesLists();
            }
        }
    }


    /// <summary>
    /// Загружает данные окна после события Loaded.
    /// </summary>
    private void LoadView() {
        LoadConfig();
    }


    /// <summary>
    /// Сохраняет настройки после подтверждения окна.
    /// </summary>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Проверяет корректность формул и обязательных настроек перед сохранением окна.
    /// </summary>
    private bool CanAcceptView() {
        bool isValid = FormulaValidator.ValidateFormulas(
            ConsumableTypes,
            SaveProperty,
            _localizationService.GetLocalizedString("MainViewModel.SavePropertyEmpty"),
            _localizationService,
            item => TryGetCategoryId(item, out int cid) ? cid : (int?) null,
            out string error);

        ErrorText = error;
        return isValid;
    }


    /// <summary>
    /// Загружает проектные настройки, настройки расходников и начальные списки назначений.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        LoadUnmodelingConfigs();
        UpdateTypesLists();

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainViewModel.SavePropertyDefault");
        _isViewLoaded = true;
    }

    /// <summary>
    /// Сохраняет параметр результата, настройки расходников и проектный конфиг плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        SaveUnmodelingConfigs();
        _pluginConfig.SaveProjectConfig();
    }

    /// <summary>
    /// Обновляет списки назначений после изменений расходников, когда представление уже загружено.
    /// </summary>
    public void RefreshAssignmentsFromConsumableTypes() {
        if(!_isViewLoaded) {
            return;
        }

        UpdateTypesLists();
    }

    /// <summary>
    /// Создает новый пользовательский расходник с новым ключом конфигурации и категорией по умолчанию.
    /// </summary>
    private void AddConsumableType() {
        int index = ConsumableTypes.Count + 1;
        string configKey = GetNextConfigKey();

        CategoryOption defaultCategory = CategoryOptions.FirstOrDefault();

        ConsumableTypes.Add(new ConsumableTypeItem {
            Title = $"Config {index}",
            ConsumableTypeName = configKey,
            ConfigKey = configKey,
            SelectedCategory = defaultCategory,
            CategoryId = defaultCategory?.Id.ToString()
        });
        CommandManager.InvalidateRequerySuggested();
        UpdateTypesLists();
    }

    /// <summary>
    /// Разрешает удаление расходника, если в коллекции есть хотя бы один элемент.
    /// </summary>
    private bool CanRemoveConsumableType(ConsumableTypeItem item) {
        return ConsumableTypes?.Count > 0;
    }

    /// <summary>
    /// Удаляет расходник из коллекции и пересобирает списки назначений.
    /// </summary>
    private void RemoveConsumableType(ConsumableTypeItem item) {
        if(ConsumableTypes == null || ConsumableTypes.Count == 0 || item == null) {
            return;
        }

        ConsumableTypeItem target = ConsumableTypes.FirstOrDefault(c => ReferenceEquals(c, item));
        if(target == null && !string.IsNullOrWhiteSpace(item.ConfigKey)) {
            target = ConsumableTypes.FirstOrDefault(c => string.Equals(c.ConfigKey, item.ConfigKey, StringComparison.OrdinalIgnoreCase));
        }

        if(target == null) {
            return;
        }

        ConsumableTypes.Remove(target);

        CommandManager.InvalidateRequerySuggested();
        UpdateTypesLists();
    }

    /// <summary>
    /// После подтверждения заменяет текущие настройки расходников настройками из файла шаблона.
    /// </summary>
    private void ResetConfigs(Window owner) {
        string message = _localizationService.GetLocalizedString("MainWindow.ResetConfirmMessage");
        string title = _localizationService.GetLocalizedString("MainWindow.Title");
        if(MessageBoxService.Show(message, 
            title, MessageBoxButton.OKCancel, 
            MessageBoxImage.Question) != MessageBoxResult.OK) {
            return;
        }

        UnmodelingSettingsDocument defaults = _revitRepository.VisSettingsStorage.GetDefaultSettings();
        LoadConsumableTypesFromSettings(defaults);
    }

    /// <summary>
    /// После подтверждения приводит шаблонные расходники к значениям шаблона и опционально добавляет отсутствующие.
    /// </summary>
    private void RefreshConfigs() {
        string title = _localizationService.GetLocalizedString("MainWindow.Title");
        string confirmMessage = _localizationService.GetLocalizedString("MainWindow.RefreshConfirmMessage");
        if(MessageBoxService.Show(
            confirmMessage,
            title,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question) != MessageBoxResult.OK) {
            return;
        }

        bool addMissingTemplateItems = true;
        if(_consumableTemplateManager.HasMissingTemplateItems()) {
            string addMissingMessage = _localizationService.GetLocalizedString(
                "MainWindow.AddMissingTemplateConsumablesConfirmMessage");
            addMissingTemplateItems = MessageBoxService.Show(
                addMissingMessage,
                title,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        _consumableTemplateManager.ApplyTemplatesToItems(addMissingTemplateItems);
        SyncLastConfigIndex();
        UpdateTypesLists();
    }

    /// <summary>
    /// Импортирует настройки расходников из JSON-файла и заменяет ими текущую коллекцию.
    /// </summary>
    private void ImportConfigs() {
        _openFileDialogService.Filter = "JSON files | *.json";
        if(!_openFileDialogService.ShowDialog()) {
            return;
        }

        try {
            string fileContent = File.ReadAllText(_openFileDialogService.File.FullName);
            UnmodelingSettingsDocument imported =
                _configSerializer.Deserialize<UnmodelingSettingsDocument>(fileContent);
            if(imported == null) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ImportError");
                return;
            }

            LoadConsumableTypesFromSettings(imported);
        } catch(JsonException) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.ImportError");
        }
    }

    /// <summary>
    /// Экспортирует текущие настройки расходников и сопутствующие настройки в JSON-файл.
    /// </summary>
    private void ExportConfigs() {
        _saveFileDialogService.AddExtension = true;
        _saveFileDialogService.Filter = "JSON files | *.json";
        _saveFileDialogService.FilterIndex = 1;

        if(!_saveFileDialogService.ShowDialog(
            _saveFileDialogService.InitialDirectory,
            "unmodeling_configs.json")) {
            return;
        }

        var exported = new UnmodelingSettingsDocument {
            UnmodelingConfig = BuildUnmodelingConfigs(),
            UnmodelingSettings = BuildUnmodelingSettings(),
            Unmodeling = _revitRepository.VisSettingsStorage.GetUnmodelingSettings()?.Unmodeling
                         ?? new Dictionary<string, UnmodelingLegacyItem>()
        };

        File.WriteAllText(
            _saveFileDialogService.File.FullName,
            _configSerializer.Serialize(exported));
    }

    /// <summary>
    /// Применяет документ настроек к модели окна и заменяет коллекцию расходников отсортированными элементами.
    /// </summary>
    private void LoadConsumableTypesFromSettings(UnmodelingSettingsDocument settings) {
        settings ??= new UnmodelingSettingsDocument();
        ApplySettings(settings);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settings.UnmodelingConfig,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = CreateSortedConsumableTypes(consumableTypes);
        UpdateTypesLists();
    }

    /// <summary>
    /// Загружает сохраненные в проекте настройки расходников из хранилища VISSettings.
    /// </summary>
    private void LoadUnmodelingConfigs() {
        UnmodelingSettingsDocument settings = _revitRepository.VisSettingsStorage.GetUnmodelingSettings();
        ApplySettings(settings);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settings?.UnmodelingConfig,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = CreateSortedConsumableTypes(consumableTypes);
    }

    /// <summary>
    /// Собирает текущие настройки расходников и сохраняет их в хранилище VISSettings.
    /// </summary>
    private void SaveUnmodelingConfigs() {
        UnmodelingSettingsDocument settings = _revitRepository.VisSettingsStorage.GetUnmodelingSettings();
        settings ??= new UnmodelingSettingsDocument();
        settings.UnmodelingConfig = BuildUnmodelingConfigs();
        settings.UnmodelingSettings = BuildUnmodelingSettings();
        _revitRepository.VisSettingsStorage.SaveUnmodelingSettings(settings);
    }

    /// <summary>
    /// Преобразует редактируемые расходники окна в словарь конфигурации для хранения и экспорта.
    /// </summary>
    private Dictionary<string, UnmodelingConfigItem> BuildUnmodelingConfigs() {
        var configs = new Dictionary<string, UnmodelingConfigItem>();
        if(ConsumableTypes == null) {
            return configs;
        }

        foreach(ConsumableTypeItem item in ConsumableTypes) {
            if(string.IsNullOrWhiteSpace(item.ConfigKey)) {
                item.ConfigKey = GetNextConfigKey();
            }

            configs[item.ConfigKey] = item.ToConfigItem();
        }

        return configs;
    }

    /// <summary>
    /// Создает новую коллекцию расходников, отсортированную по ключу конфигурации.
    /// </summary>
    private static ObservableCollection<ConsumableTypeItem> CreateSortedConsumableTypes(
        IEnumerable<ConsumableTypeItem> consumableTypes) {
        return new ObservableCollection<ConsumableTypeItem>(
            (consumableTypes ?? Enumerable.Empty<ConsumableTypeItem>())
                .OrderBy(item => item?.ConfigKey ?? string.Empty, StringComparer.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Собирает дополнительные настройки раздела немоделируемых элементов.
    /// </summary>
    private UnmodelingSettingsOptions BuildUnmodelingSettings() {
        return new UnmodelingSettingsOptions {
            OnlyProjectInstances = OnlyPlacedInProject
        };
    }

    /// <summary>
    /// Применяет дополнительные настройки документа к состоянию окна.
    /// </summary>
    private void ApplySettings(UnmodelingSettingsDocument settings) {
        if(settings?.UnmodelingSettings != null) {
            OnlyPlacedInProject = settings.UnmodelingSettings.OnlyProjectInstances;
        }
    }

    /// <summary>
    /// Возвращает следующий ключ конфигурации вида config_NNN и продвигает внутренний счетчик.
    /// </summary>
    private string GetNextConfigKey() {
        _lastConfigIndex++;
        return $"config_{_lastConfigIndex:000}";
    }

    /// <summary>
    /// Синхронизирует внутренний счетчик ключей после массового добавления или замены расходников.
    /// </summary>
    private void SyncLastConfigIndex() {
        int lastConfigIndex = 0;
        foreach(ConsumableTypeItem item in ConsumableTypes ?? Enumerable.Empty<ConsumableTypeItem>()) {
            Match match = Regex.Match(item?.ConfigKey ?? string.Empty, @"config_(\d+)", RegexOptions.IgnoreCase);
            if(match.Success && int.TryParse(match.Groups[1].Value, out int index) && index > lastConfigIndex) {
                lastConfigIndex = index;
            }
        }

        _lastConfigIndex = lastConfigIndex;
    }

    /// <summary>
    /// Строит дерево назначений: категории Revit, типы систем и доступные для них конфигурации расходников.
    /// </summary>
    private void UpdateTypesLists() {
        CategoryAssignments = _categoryAssignmentBuilder.Build(
            CategoryOptions,
            ConsumableTypes,
            OnlyPlacedInProject,
            ResolveCategoryId);
    }

    /// <summary>
    /// Пытается получить числовой идентификатор категории из выбранной категории или сохраненного строкового значения.
    /// </summary>
    private static bool TryGetCategoryId(ConsumableTypeItem item, out int categoryId) {
        if(item?.SelectedCategory != null) {
            categoryId = item.SelectedCategory.Id;
            return true;
        }

        if(!string.IsNullOrWhiteSpace(item?.CategoryId) && int.TryParse(item.CategoryId, out int parsed)) {
            categoryId = parsed;
            return true;
        }

        categoryId = 0;
        return false;
    }

    /// <summary>
    /// Возвращает nullable-идентификатор категории расходника для валидатора формул.
    /// </summary>
    private int? ResolveCategoryId(ConsumableTypeItem item) {
        return TryGetCategoryId(item, out int cid) ? cid : (int?) null;
    }

    /// <summary>
    /// Преобразует сохраненное значение категории в один из доступных вариантов выбора.
    /// </summary>
    private CategoryOption ResolveCategoryOption(string categoryValue) {
        return _categoryOptionsProvider.ResolveCategoryOption(CategoryOptions, categoryValue);
    }
}
