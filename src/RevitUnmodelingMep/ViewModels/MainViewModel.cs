using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

using Autodesk.Revit.DB;

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
        _categoryOptions = CreateCategoryOptions();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddConsumableTypeCommand = RelayCommand.Create(AddConsumableType);
        RemoveConsumableTypeCommand = RelayCommand.Create<ConsumableTypeItem>(RemoveConsumableType, CanRemoveConsumableType);
        ImportConfigsCommand = RelayCommand.Create(ImportConfigs);
        ExportConfigsCommand = RelayCommand.Create(ExportConfigs);
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

    public ICommand ResetConfigsCommand { get; }

    public HintPanelViewModel Hint { get; }
    public IOpenFileDialogService OpenFileDialogService => _openFileDialogService;
    public ISaveFileDialogService SaveFileDialogService => _saveFileDialogService;


    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    public ObservableCollection<ConsumableTypeItem> ConsumableTypes {
        get => _consumableTypes;
        set => RaiseAndSetIfChanged(ref _consumableTypes, value);
    }

    public ObservableCollection<CategoryAssignmentItem> CategoryAssignments {
        get => _categoryAssignments;
        set => RaiseAndSetIfChanged(ref _categoryAssignments, value);
    }

    public IReadOnlyList<CategoryOption> CategoryOptions => _categoryOptions;
    public bool IsViewLoaded => _isViewLoaded;

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


    private void LoadView() {
        LoadConfig();
    }


    private void AcceptView() {
        SaveConfig();
    }

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


    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        LoadUnmodelingConfigs();
        UpdateTypesLists();

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainViewModel.SavePropertyDefault");
        _isViewLoaded = true;
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        SaveUnmodelingConfigs();
        _pluginConfig.SaveProjectConfig();
    }

    internal IProgressDialogService CreatePercentProgressDialog(string titleKey) {
        var dialog = GetPlatformService<IProgressDialogService>();
        dialog.StepValue = 1;
        dialog.DisplayTitleFormat =
            $"{_localizationService.GetLocalizedString(titleKey)} [{{0}}%]";
        dialog.MaxValue = 100;
        dialog.Show();
        return dialog;
    }

    public void RefreshAssignmentsFromConsumableTypes() {
        if(!_isViewLoaded) {
            return;
        }

        UpdateTypesLists();
    }

    private void AddConsumableType() {
        int index = ConsumableTypes.Count + 1;
        string configKey = GetNextConfigKey();

        CategoryOption defaultCategory = CategoryOptions.FirstOrDefault();

        ConsumableTypes.Add(new ConsumableTypeItem {
            Title = $"Config {index}",
            ConsumableTypeName = configKey,
            ConfigKey = configKey,
            RawConfig = new JObject(),
            AssignedElementIds = new JArray(),
            SelectedCategory = defaultCategory,
            CategoryId = defaultCategory?.Id.ToString()
        });
        CommandManager.InvalidateRequerySuggested();
        UpdateTypesLists();
    }

    private bool CanRemoveConsumableType(ConsumableTypeItem item) {
        return ConsumableTypes?.Count > 0;
    }

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

    private void LoadConsumableTypesFromSettings(UnmodelingSettingsDocument settings) {
        settings ??= new UnmodelingSettingsDocument();
        ApplySettings(settings);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settings.UnmodelingConfig,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>(consumableTypes);
        UpdateTypesLists();
    }

    private void LoadUnmodelingConfigs() {
        UnmodelingSettingsDocument settings = _revitRepository.VisSettingsStorage.GetUnmodelingSettings();
        ApplySettings(settings);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settings?.UnmodelingConfig,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>(consumableTypes);
    }

    private void SaveUnmodelingConfigs() {
        UnmodelingSettingsDocument settings = _revitRepository.VisSettingsStorage.GetUnmodelingSettings();
        settings ??= new UnmodelingSettingsDocument();
        settings.UnmodelingConfig = BuildUnmodelingConfigs();
        settings.UnmodelingSettings = BuildUnmodelingSettings();
        _revitRepository.VisSettingsStorage.SaveUnmodelingSettings(settings);
    }

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

    private UnmodelingSettingsOptions BuildUnmodelingSettings() {
        return new UnmodelingSettingsOptions {
            OnlyProjectInstances = OnlyPlacedInProject
        };
    }

    private void ApplySettings(UnmodelingSettingsDocument settings) {
        if(settings?.UnmodelingSettings != null) {
            OnlyPlacedInProject = settings.UnmodelingSettings.OnlyProjectInstances;
        }
    }

    private string GetNextConfigKey() {
        _lastConfigIndex++;
        return $"config_{_lastConfigIndex:000}";
    }

    private void UpdateTypesLists() {
        List<BuiltInCategory> categories = new List<BuiltInCategory> {
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_DuctInsulations,
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_PipingSystem
        };

        var assignments = new List<CategoryAssignmentItem>();

        foreach(BuiltInCategory builtInCategory in categories) {
            CategoryOption option = CategoryOptions.FirstOrDefault(o => o.BuiltInCategory == builtInCategory);
            int optionCategoryId = option?.Id ?? (int) builtInCategory;
            string categoryName = option?.Name ?? builtInCategory.ToString();

            List<Element> types = CollectionGenerator.GetElementTypesByCategory(_revitRepository.Doc, builtInCategory) 
                ?? new List<Element>();
            if(types.Count == 0) {
                continue;
            }

            List<ConsumableTypeItem> configsForCategory = ConsumableTypes?
                .Where(c => TryGetCategoryId(c, out int cid) && cid == optionCategoryId)
                .OrderBy(c => c?.ConsumableTypeName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ToList() ?? new List<ConsumableTypeItem>();

            HashSet<int> placedTypeIds = OnlyPlacedInProject ? GetPlacedTypeIds(builtInCategory) : null;

            ObservableCollection<SystemTypeItem> systemTypes =
                new ObservableCollection<SystemTypeItem>(
                    types
                        .OfType<ElementType>()
                        .Where(type => placedTypeIds == null || placedTypeIds.Contains(GetElementIdValue(type.Id)))
                        .OrderBy(type => type?.Name ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                        .Select(type => CreateSystemTypeItem(type, configsForCategory)));

            if(systemTypes.Count == 0) {
                continue;
            }

            assignments.Add(new CategoryAssignmentItem {
                Name = categoryName,
                Category = builtInCategory,
                SystemTypes = systemTypes
            });
        }

        CategoryAssignments = new ObservableCollection<CategoryAssignmentItem>(
            assignments.OrderBy(a => a?.Name ?? string.Empty, StringComparer.CurrentCultureIgnoreCase));
    }

    private HashSet<int> GetPlacedTypeIds(BuiltInCategory builtInCategory) {
        var result = new HashSet<int>();

        var collector = new FilteredElementCollector(_revitRepository.Doc)
            .OfCategory(builtInCategory)
            .WhereElementIsNotElementType();

        foreach(Element element in collector) {
            ElementId typeId = element.GetTypeId();
            if(typeId == null || typeId == ElementId.InvalidElementId) {
                continue;
            }

            result.Add(GetElementIdValue(typeId));
        }

        return result;
    }

    private SystemTypeItem CreateSystemTypeItem(ElementType elementType, List<ConsumableTypeItem> configs) {
        int typeId = GetElementIdValue(elementType.Id);

        ObservableCollection<ConfigAssignmentItem> configAssignments =
            new ObservableCollection<ConfigAssignmentItem>(
                configs.Select(config => new ConfigAssignmentItem(config, typeId)));

        return new SystemTypeItem {
            Name = elementType.Name,
            Id = typeId,
            Configs = configAssignments
        };
    }

    private static int GetElementIdValue(ElementId elementId) {
        long value;
#if REVIT_2024_OR_GREATER
        value = elementId?.Value ?? 0;
#else
        value = elementId?.IntegerValue ?? 0;
#endif
        return unchecked((int) value);
    }

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

    private int? ResolveCategoryId(ConsumableTypeItem item) {
        return TryGetCategoryId(item, out int cid) ? cid : (int?) null;
    }

    private IReadOnlyList<CategoryOption> CreateCategoryOptions() {
        List<CategoryOption> options = new List<CategoryOption> {
            
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctsName"), 
                BuiltInCategory.OST_DuctCurves),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipesName"), 
                BuiltInCategory.OST_PipeCurves),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipeInsName"), 
                BuiltInCategory.OST_PipeInsulations),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctInsName"), 
                BuiltInCategory.OST_DuctInsulations),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctSysName"), 
                BuiltInCategory.OST_DuctSystem),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipeSysName"), 
                BuiltInCategory.OST_PipingSystem)
        };

        return options;
    }

    private CategoryOption CreateCategoryOption(string name, BuiltInCategory builtInCategory) {
        Category category = Category.GetCategory(_revitRepository.Document, builtInCategory);

        long idValue;
#if REVIT_2024_OR_GREATER
        idValue = category?.Id.Value ?? (long) (int) builtInCategory;
#else
        idValue = category?.Id.IntegerValue ?? (int) builtInCategory;
#endif
        int id = unchecked((int) idValue);

        return new CategoryOption {
            Name = name,
            BuiltInCategory = builtInCategory,
            Id = id
        };
    }

    private CategoryOption ResolveCategoryOption(string categoryValue) {
        if(string.IsNullOrWhiteSpace(categoryValue)) {
            return CategoryOptions.FirstOrDefault();
        }

        if(int.TryParse(categoryValue, out int id)) {
            return CategoryOptions.FirstOrDefault(o => o.Id == id);
        }

        string trimmed = categoryValue.Trim();
        if(trimmed.StartsWith("BuiltInCategory.", StringComparison.OrdinalIgnoreCase)) {
            string enumName = trimmed.Substring("BuiltInCategory.".Length);
            CategoryOption byEnumName = CategoryOptions.FirstOrDefault(o =>
                string.Equals(o.BuiltInCategory.ToString(), enumName, StringComparison.OrdinalIgnoreCase));
            if(byEnumName != null) {
                return byEnumName;
            }
        }

        return CategoryOptions.FirstOrDefault(o =>
            string.Equals(o.Name, categoryValue, StringComparison.OrdinalIgnoreCase))
               ?? CategoryOptions.FirstOrDefault();
    }
}
