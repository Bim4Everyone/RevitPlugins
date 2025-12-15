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

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private const string _unmodelingConfigKey = "UNMODELING_CONFIG";

    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private string _saveProperty;
    private ObservableCollection<ConsumableTypeItem> _consumableTypes;
    private int _lastConfigIndex;
    private readonly IReadOnlyList<CategoryOption> _categoryOptions;
    
    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
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
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }
    
    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public ICommand AddConsumableTypeCommand { get; }

    public ICommand RemoveConsumableTypeCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Свойство для примера. (требуется удалить)
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    public ObservableCollection<ConsumableTypeItem> ConsumableTypes {
        get => _consumableTypes;
        set => RaiseAndSetIfChanged(ref _consumableTypes, value);
    }

    public IReadOnlyList<CategoryOption> CategoryOptions => _categoryOptions;

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        LoadUnmodelingConfigs();

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
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
    }

    internal class ConsumableTypeItem : BaseViewModel {
        private string _title;
        private string _selectedType;
        private string _name;
        private string _grouping;
        private string _naming;
        private string _brand;
        private string _code;
        private string _unit;
        private string _factory;
        private string _numberFormula;
        private string _noteFormat;
        private string _enamel;
        private string _primer;
        private CategoryOption _selectedCategory;

        public string Title {
            get => _title;
            set => RaiseAndSetIfChanged(ref _title, value);
        }

        public string ConfigKey { get; set; }

        public string SelectedType {
            get => _selectedType;
            set => RaiseAndSetIfChanged(ref _selectedType, value);
        }

        public string Name {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        public string CategoryId { get; set; }

        public string Grouping {
            get => _grouping;
            set => RaiseAndSetIfChanged(ref _grouping, value);
        }

        public string Naming {
            get => _naming;
            set => RaiseAndSetIfChanged(ref _naming, value);
        }

        public string Brand {
            get => _brand;
            set => RaiseAndSetIfChanged(ref _brand, value);
        }

        public string Code {
            get => _code;
            set => RaiseAndSetIfChanged(ref _code, value);
        }

        public string Unit {
            get => _unit;
            set => RaiseAndSetIfChanged(ref _unit, value);
        }

        public string Factory {
            get => _factory;
            set => RaiseAndSetIfChanged(ref _factory, value);
        }

        public string NumberFormula {
            get => _numberFormula;
            set => RaiseAndSetIfChanged(ref _numberFormula, value);
        }

        public string NoteFormat {
            get => _noteFormat;
            set => RaiseAndSetIfChanged(ref _noteFormat, value);
        }

        public string Enamel {
            get => _enamel;
            set => RaiseAndSetIfChanged(ref _enamel, value);
        }

        public string Primer {
            get => _primer;
            set => RaiseAndSetIfChanged(ref _primer, value);
        }

        public CategoryOption SelectedCategory {
            get => _selectedCategory;
            set {
                RaiseAndSetIfChanged(ref _selectedCategory, value);
                CategoryId = value?.Id.ToString();
            }
        }

        public JArray AssignedElementIds { get; set; } = new JArray();

        public JObject RawConfig { get; set; } = new JObject();

        public static ConsumableTypeItem FromConfig(JProperty configProperty) {
            JObject value = configProperty.Value as JObject ?? new JObject();
            JObject clonedValue = (JObject) value.DeepClone();
            JArray assignedIds = value["ASSIGNED_ELEMENT_IDS"] as JArray ?? new JArray();

            return new ConsumableTypeItem {
                ConfigKey = configProperty.Name,
                Title = (string) value["CONFIG_NAME"],
                Name = (string) value["CONFIG_NAME"],
                Naming = (string) value["NAME"],
                CategoryId = (string) value["CATEGORY"],
                Grouping = (string) value["GROUP"],
                Brand = (string) value["MARK"],
                Code = (string) value["CODE"],
                Unit = (string) value["UNIT"],
                Factory = (string) value["CREATOR"],
                NumberFormula = (string) value["VALUE_FORMULA"],
                NoteFormat = (string) value["NOTE_FORMAT"],
                AssignedElementIds = new JArray(assignedIds),
                RawConfig = clonedValue
            };
        }

        public JObject ToJObject() {
            JObject result = RawConfig != null
                ? (JObject) RawConfig.DeepClone()
                : new JObject();

            result["CONFIG_NAME"] = Name ?? string.Empty;
            result["NAME"] = Naming ?? string.Empty;
            result["CATEGORY"] = CategoryId ?? string.Empty;
            result["GROUP"] = Grouping ?? string.Empty;
            result["MARK"] = Brand ?? string.Empty;
            result["CODE"] = Code ?? string.Empty;
            result["UNIT"] = Unit ?? string.Empty;
            result["CREATOR"] = Factory ?? string.Empty;
            result["VALUE_FORMULA"] = NumberFormula ?? string.Empty;
            result["NOTE_FORMAT"] = NoteFormat ?? string.Empty;
            result["ASSIGNED_ELEMENT_IDS"] = AssignedElementIds ?? new JArray();

            return result;
        }
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

    private void UpdateConfigIndex(string configKey) {
        Match match = Regex.Match(configKey ?? string.Empty, @"config_(\d+)", RegexOptions.IgnoreCase);
        if(match.Success && int.TryParse(match.Groups[1].Value, out int value) && value > _lastConfigIndex) {
            _lastConfigIndex = value;
        }
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

        // Support legacy string like "BuiltInCategory.OST_PipeCurves"
        string trimmed = categoryValue.Trim();
        if(trimmed.StartsWith("BuiltInCategory.", StringComparison.OrdinalIgnoreCase)) {
            string enumName = trimmed.Substring("BuiltInCategory.".Length);
            CategoryOption byEnumName = CategoryOptions.FirstOrDefault(o =>
                string.Equals(o.BuiltInCategory.ToString(), enumName, StringComparison.OrdinalIgnoreCase));
            if(byEnumName != null) {
                return byEnumName;
            }
        }

        // Fallback: match by display name
        return CategoryOptions.FirstOrDefault(o =>
            string.Equals(o.Name, categoryValue, StringComparison.OrdinalIgnoreCase))
               ?? CategoryOptions.FirstOrDefault();
    }

    internal class CategoryOption {
        public string Name { get; set; }
        public BuiltInCategory BuiltInCategory { get; set; }
        public int Id { get; set; }
    }
}
