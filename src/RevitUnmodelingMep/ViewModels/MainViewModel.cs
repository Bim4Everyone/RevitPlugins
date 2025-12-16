using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Newtonsoft.Json.Linq;

using Autodesk.Revit.DB;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;


internal class MainViewModel : BaseViewModel {
    private const string _unmodelingConfigKey = "UNMODELING_CONFIG";

    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;
    private ObservableCollection<ConsumableTypeItem> _consumableTypes;
    private ObservableCollection<CategoryAssignmentItem> _categoryAssignments;
    private int _lastConfigIndex;
    private readonly IReadOnlyList<CategoryOption> _categoryOptions;
    

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

        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>();
        CategoryAssignments = new ObservableCollection<CategoryAssignmentItem>();
    }


    public ICommand LoadViewCommand { get; }
    

    public ICommand AcceptViewCommand { get; }

    public ICommand AddConsumableTypeCommand { get; }

    public ICommand RemoveConsumableTypeCommand { get; }


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


    private void LoadView() {
        LoadConfig();
    }


    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }


    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        LoadUnmodelingConfigs();
        UpdateTypesLists();

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
        SaveUnmodelingConfigs();
        _pluginConfig.SaveProjectConfig();
    }

    private void AddConsumableType() {
        int index = ConsumableTypes.Count + 1;
        string configKey = GetNextConfigKey();

        CategoryOption defaultCategory = CategoryOptions.FirstOrDefault();

        ConsumableTypes.Add(new ConsumableTypeItem {
            Title = $"Config {index}",
            Name = configKey,
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

        ConsumableTypes.RemoveAt(ConsumableTypes.Count - 1);

        CommandManager.InvalidateRequerySuggested();
        UpdateTypesLists();
    }

    private void LoadUnmodelingConfigs() {
        JObject settings = _revitRepository.SettingsUpdaterWorker.GetUnmodelingConfig();
        IEnumerable<ConsumableTypeItem> consumableTypes = GetConsumableItems(settings);
        ConsumableTypes = new ObservableCollection<ConsumableTypeItem>(consumableTypes);
    }

    private IEnumerable<ConsumableTypeItem> GetConsumableItems(JObject settings) {
        _lastConfigIndex = 0;
        if(settings == null) {
            yield break;
        }

        if(settings.TryGetValue(_unmodelingConfigKey, out JToken configToken)
           && configToken is JObject configObj) {
            foreach(JProperty property in configObj.Properties()) {
                UpdateConfigIndex(property.Name);
                ConsumableTypeItem item = ConsumableTypeItem.FromConfig(property);
                item.SelectedCategory = ResolveCategoryOption(item.CategoryId);
                yield return item;
            }
        }
    }

    private void SaveUnmodelingConfigs() {
        JObject configs = BuildUnmodelingConfigs();
        _revitRepository.SettingsUpdaterWorker.SetSettingValue(
            new List<string> { _unmodelingConfigKey },
            configs);
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

        var assignments = new ObservableCollection<CategoryAssignmentItem>();

        foreach(BuiltInCategory builtInCategory in categories) {
            CategoryOption option = CategoryOptions.FirstOrDefault(o => o.BuiltInCategory == builtInCategory);
            int optionCategoryId = option?.Id ?? (int) builtInCategory;
            string categoryName = option?.Name ?? builtInCategory.ToString();

            List<Element> types = _revitRepository.GetElementsByCategory(builtInCategory) ?? new List<Element>();
            if(types.Count == 0) {
                continue;
            }

            List<ConsumableTypeItem> configsForCategory = ConsumableTypes?
                .Where(c => TryGetCategoryId(c, out int cid) && cid == optionCategoryId)
                .ToList() ?? new List<ConsumableTypeItem>();

            ObservableCollection<SystemTypeItem> systemTypes =
                new ObservableCollection<SystemTypeItem>(
                    types
                        .OfType<ElementType>()
                        .Select(type => CreateSystemTypeItem(type, configsForCategory)));

            assignments.Add(new CategoryAssignmentItem {
                Name = categoryName,
                Category = builtInCategory,
                SystemTypes = systemTypes
            });
        }

        CategoryAssignments = assignments;
    }

    private void UpdateConfigIndex(string configKey) {
        Match match = Regex.Match(configKey ?? string.Empty, @"config_(\d+)", RegexOptions.IgnoreCase);
        if(match.Success && int.TryParse(match.Groups[1].Value, out int value) && value > _lastConfigIndex) {
            _lastConfigIndex = value;
        }
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
            CreateCategoryOption("Воздуховоды", BuiltInCategory.OST_DuctCurves),
            CreateCategoryOption("Трубы", BuiltInCategory.OST_PipeCurves),
            CreateCategoryOption("Материалы изоляции трубопроводов", BuiltInCategory.OST_PipeInsulations),
            CreateCategoryOption("Материалы изоляции воздуховодов", BuiltInCategory.OST_DuctInsulations),
            CreateCategoryOption("Системы воздуховодов", BuiltInCategory.OST_DuctSystem),
            CreateCategoryOption("Системы трубопроводов", BuiltInCategory.OST_PipingSystem)
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
