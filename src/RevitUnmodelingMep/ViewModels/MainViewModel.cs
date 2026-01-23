using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Newtonsoft.Json.Linq;

using Autodesk.Revit.DB;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;


internal class MainViewModel : BaseViewModel {
    private const string _unmodelingSettingsKey = "UNMODELING_SETTINGS";
    private const string _onlyProjectInstancesKey = "ONLY_PROJECT_INSTANCES";

    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;
    private ObservableCollection<ConsumableTypeItem> _consumableTypes;
    private ObservableCollection<CategoryAssignmentItem> _categoryAssignments;
    private bool _onlyPlacedInProject = true;
    private int _lastConfigIndex;
    private readonly IReadOnlyList<CategoryOption> _categoryOptions;
    private bool _isViewLoaded;
    private bool _isFormulaEditing;
    private bool _isNameEditing;
    private bool _isNoteEditing;
    private ConsumableTypeItem _activeFormulaItem;
    private ObservableCollection<string> _activeHintItems = new ObservableCollection<string>();
    

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
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
    }


    public ICommand LoadViewCommand { get; }
    

    public ICommand AcceptViewCommand { get; }

    public ICommand AddConsumableTypeCommand { get; }

    public ICommand RemoveConsumableTypeCommand { get; }

    public ICommand ImportConfigsCommand { get; }

    public ICommand ExportConfigsCommand { get; }

    public ICommand ResetConfigsCommand { get; }


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

    public bool IsFormulaEditing {
        get => _isFormulaEditing;
        set {
            RaiseAndSetIfChanged(ref _isFormulaEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsNameEditing {
        get => _isNameEditing;
        set {
            RaiseAndSetIfChanged(ref _isNameEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsNoteEditing {
        get => _isNoteEditing;
        set {
            RaiseAndSetIfChanged(ref _isNoteEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsHintVisible => IsFormulaEditing || IsNameEditing || IsNoteEditing;

    public string HintTitle => _localizationService.GetLocalizedString("MainWindow.FormulaVariables");

    public bool IsHintInfoVisible => IsNameEditing || IsNoteEditing;

    public string HintInfoText => IsHintInfoVisible
        ? _localizationService.GetLocalizedString("MainWindow.PlaceholderBracesHint")
        : string.Empty;

    public ObservableCollection<string> ActiveHintItems {
        get => _activeHintItems;
        private set => RaiseAndSetIfChanged(ref _activeHintItems, value);
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

    public void BeginFormulaEdit(ConsumableTypeItem item) {
        IsFormulaEditing = item != null;
        IsNameEditing = false;
        IsNoteEditing = false;
        SetActiveFormulaItem(item);
    }

    public void EndFormulaEdit(ConsumableTypeItem item) {
        if(_activeFormulaItem != item) {
            return;
        }

        IsFormulaEditing = false;
        if(!IsNameEditing && !IsNoteEditing) {
            SetActiveFormulaItem(null);
        }
    }

    public void BeginNameEdit(ConsumableTypeItem item) {
        IsNameEditing = item != null;
        IsFormulaEditing = false;
        IsNoteEditing = false;
        SetActiveFormulaItem(item);
    }

    public void EndNameEdit(ConsumableTypeItem item) {
        if(_activeFormulaItem != item) {
            return;
        }

        IsNameEditing = false;
        if(!IsFormulaEditing && !IsNoteEditing) {
            SetActiveFormulaItem(null);
        }
    }

    public void BeginNoteEdit(ConsumableTypeItem item) {
        IsNoteEditing = item != null;
        IsFormulaEditing = false;
        IsNameEditing = false;
        SetActiveFormulaItem(item);
    }

    public void EndNoteEdit(ConsumableTypeItem item) {
        if(_activeFormulaItem != item) {
            return;
        }

        IsNoteEditing = false;
        if(!IsFormulaEditing && !IsNameEditing) {
            SetActiveFormulaItem(null);
        }
    }

    private void SetActiveFormulaItem(ConsumableTypeItem item) {
        if(ReferenceEquals(_activeFormulaItem, item)) {
            UpdateActiveHintItems();
            return;
        }

        if(_activeFormulaItem != null) {
            _activeFormulaItem.PropertyChanged -= ActiveFormulaItemOnPropertyChanged;
        }

        _activeFormulaItem = item;

        if(_activeFormulaItem != null) {
            _activeFormulaItem.PropertyChanged += ActiveFormulaItemOnPropertyChanged;
        }

        UpdateActiveHintItems();
    }

    private void ActiveFormulaItemOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(ConsumableTypeItem.SelectedCategory)
           || e.PropertyName == nameof(ConsumableTypeItem.CategoryId)) {
            UpdateActiveHintItems();
        }
    }

    private void UpdateActiveHintItems() {
        if(IsNoteEditing) {
            ActiveHintItems = new ObservableCollection<string>(FormulaValidator.GetAllowedNoteTokens());
            return;
        }

        if(_activeFormulaItem == null) {
            ActiveHintItems = new ObservableCollection<string>();
            return;
        }

        if(!TryGetCategoryId(_activeFormulaItem, out int categoryId)) {
            ActiveHintItems = new ObservableCollection<string>();
            return;
        }

        var props = FormulaValidator.GetAllowedPropertyNames(categoryId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ActiveHintItems = new ObservableCollection<string>(props);
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
        if(ConsumableTypes == null || ConsumableTypes.Count == 0) {
            return;
        }

        if(item != null && ConsumableTypes.Contains(item)) {
            ConsumableTypes.Remove(item);
        } else {
            ConsumableTypes.RemoveAt(ConsumableTypes.Count - 1);
        }

        CommandManager.InvalidateRequerySuggested();
        UpdateTypesLists();
    }

    private void ResetConfigs(Window owner) {
        string message = _localizationService.GetLocalizedString("MainWindow.ResetConfirmMessage");
        string title = _localizationService.GetLocalizedString("MainWindow.Title");
        if(MessageBox.Show(owner ?? Application.Current?.MainWindow, message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) {
            return;
        }

        JObject defaults = _revitRepository.VisSettingsStorage.GetDefaultSettings();
        LoadConsumableTypesFromSettings(defaults);
    }

    private void ImportConfigs() {
        var dialog = new OpenFileDialog {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if(dialog.ShowDialog() != true) {
            return;
        }

        try {
            string fileContent = File.ReadAllText(dialog.FileName);
            JObject imported = JObject.Parse(fileContent);
            LoadConsumableTypesFromSettings(imported);
        } catch {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.ImportError");
        }
    }

    private void ExportConfigs() {
        var dialog = new SaveFileDialog {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "unmodeling_configs.json"
        };

        if(dialog.ShowDialog() != true) {
            return;
        }

        var exported = new JObject {
            [UnmodelingConfigReader.UnmodelingConfigKey] = BuildUnmodelingConfigs(),
            [_unmodelingSettingsKey] = BuildUnmodelingSettings()
        };
        File.WriteAllText(dialog.FileName, exported.ToString());
    }

    private void LoadConsumableTypesFromSettings(JObject settings) {
        JObject settingsWithKey = NormalizeSettings(settings);
        ApplySettings(settingsWithKey);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settingsWithKey,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>(consumableTypes);
        UpdateTypesLists();
    }

    private void LoadUnmodelingConfigs() {
        JObject settings = _revitRepository.VisSettingsStorage.GetUnmodelingConfig();
        settings = NormalizeSettings(settings);
        ApplySettings(settings);

        IReadOnlyList<ConsumableTypeItem> consumableTypes =
            UnmodelingConfigReader.GetConsumableItems(
                settings,
                ResolveCategoryOption,
                out _lastConfigIndex);

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>(consumableTypes);
    }

    private void SaveUnmodelingConfigs() {
        JObject configs = BuildUnmodelingConfigs();
        _revitRepository.VisSettingsStorage.SetSettingValue(
            new List<string> { UnmodelingConfigReader.UnmodelingConfigKey },
            configs);

        _revitRepository.VisSettingsStorage.SetSettingValue(
            new List<string> { _unmodelingSettingsKey },
            BuildUnmodelingSettings());
    }

    private JObject BuildUnmodelingConfigs() {
        JObject configs = new JObject();
        if(ConsumableTypes == null) {
            return configs;
        }

        foreach(ConsumableTypeItem item in ConsumableTypes) {
            if(string.IsNullOrWhiteSpace(item.ConfigKey)) {
                item.ConfigKey = GetNextConfigKey();
            }

            configs[item.ConfigKey] = item.ToJObject();
        }

        return configs;
    }

    private JObject BuildUnmodelingSettings() {
        return new JObject {
            [_onlyProjectInstancesKey] = OnlyPlacedInProject
        };
    }

    private static JObject NormalizeSettings(JObject settings) {
        if(settings?.ContainsKey(UnmodelingConfigReader.UnmodelingConfigKey) == true) {
            return settings;
        }

        return new JObject {
            [UnmodelingConfigReader.UnmodelingConfigKey] = settings ?? new JObject()
        };
    }

    private void ApplySettings(JObject settings) {
        if(settings == null) {
            return;
        }

        if(settings.TryGetValue(_unmodelingSettingsKey, out JToken settingsToken)
           && settingsToken is JObject settingsObj
           && settingsObj.TryGetValue(_onlyProjectInstancesKey, out JToken onlyProjectToken)
           && onlyProjectToken.Type == JTokenType.Boolean) {
            OnlyPlacedInProject = (bool) onlyProjectToken;
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
