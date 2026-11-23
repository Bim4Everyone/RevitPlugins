using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class PlacementSettingsViewModel : BaseViewModel {
    private readonly RevitRepository _repository;
    private readonly SystemPluginConfig _systemConfig;
    private readonly ILocalizationService _localization;
    private readonly ILintelConfigRuleStorage _storage;
    private readonly ILintelConfigRuleSerializer _serializer;
    private readonly ISettingsDialogService _dialogs;
    private readonly ISettingsCommandFactory _commandFactory;
    private readonly IOpenFileDialogService _openDialog;
    private readonly ISaveFileDialogService _saveDialog;
    private readonly IMessageBoxService _messages;
    private readonly FamilySettingsViewModel _familySettings;
    private readonly GeneralSettingsViewModel _generalSettings;
    private readonly HashSet<string> _deletedConfigs = new(StringComparer.OrdinalIgnoreCase);
    private readonly LintelManagerSettings _settings;
    private LintelConfigRuleViewModel _selectedConfig;
    private ObservableCollection<string> _lintelTypes;
    private bool _updating;

    public PlacementSettingsViewModel(RevitRepository repository, SystemPluginConfig systemConfig,
        ILocalizationService localization, ILintelConfigRuleStorage storage, ILintelConfigRuleSerializer serializer,
        ILintelConfigRuleResolver resolver, ISettingsDialogService dialogs, IOpenFileDialogService openDialog,
        ISaveFileDialogService saveDialog, IMessageBoxService messages, FamilySettingsViewModel familySettings,
        GeneralSettingsViewModel generalSettings, LintelManagerSettings settings,
        ISettingsCommandFactory commandFactory) {
        _repository = repository;
        _systemConfig = systemConfig;
        _localization = localization;
        _storage = storage;
        _serializer = serializer;
        _dialogs = dialogs;
        _commandFactory = commandFactory;
        _openDialog = openDialog;
        _saveDialog = saveDialog;
        _messages = messages;
        _familySettings = familySettings;
        _generalSettings = generalSettings;
        _settings = settings;
        CreateConfigCommand = commandFactory.CreateAsync(CreateConfigAsync);
        ImportConfigCommand = commandFactory.CreateAsync(ImportConfigAsync);
        SaveConfigCommand = commandFactory.Create(SaveConfig, () => CanEditRules);
        SaveAsConfigCommand = commandFactory.Create(SaveAsConfig, () => SelectedConfig is not null);
        DeleteConfigCommand = commandFactory.Create(DeleteConfig, () => CanEditRules);
        CopyConfigCommand = commandFactory.Create(CopyConfig, () => SelectedConfig is not null);
        AddPlaceRuleCommand = commandFactory.CreateAsync(AddPlaceRuleAsync, () => CanEditRules);
        RenamePlaceRuleCommand = commandFactory.CreateAsync<LintelPlaceRuleViewModel>(RenamePlaceRuleAsync,
            rule => CanEditRules && SelectedConfig.PlaceRules.Contains(rule));
        DeletePlaceRuleCommand = commandFactory.Create<LintelPlaceRuleViewModel>(DeletePlaceRule,
            rule => CanEditRules && SelectedConfig.PlaceRules.Contains(rule));
        AddRuleCommand = commandFactory.Create(AddRule, CanAddRule);
        DeleteRuleCommand = commandFactory.Create<LintelRuleViewModel>(DeleteRule,
            rule => CanEditRules && SelectedConfig.SelectedRule?.Rules.Contains(rule) == true);
        foreach(var config in resolver.LintelConfigRules) {
            Configs.Add(CreateConfigViewModel(config));
        }
        if(!string.IsNullOrWhiteSpace(settings.LintelConfigRuleName)
            && !Configs.Any(config => SameName(config.Name, settings.LintelConfigRuleName))) {
            Configs.Add(CreateConfigViewModel(new LintelConfigRule {
                RuleConfigName = settings.LintelConfigRuleName, LintelPlaceRules = []
            }));
        }
        SelectedConfig = Configs.FirstOrDefault(config => SameName(config.Name, settings.LintelConfigRuleName))
                         ?? Configs.FirstOrDefault();
        RefreshLintelTypes();
        RefreshWalls();
        _familySettings.SelectionChanged += RefreshLintelTypes;
        _generalSettings.StructureWallSelectionChanged += RefreshWalls;
    }

    public ObservableCollection<LintelConfigRuleViewModel> Configs { get; } = [];
    public ICommand CreateConfigCommand { get; }
    public ICommand ImportConfigCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand SaveAsConfigCommand { get; }
    public ICommand DeleteConfigCommand { get; }
    public ICommand CopyConfigCommand { get; }
    public ICommand AddPlaceRuleCommand { get; }
    public ICommand RenamePlaceRuleCommand { get; }
    public ICommand DeletePlaceRuleCommand { get; }
    public ICommand AddRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public bool CanEditRules => SelectedConfig is not null && !SelectedConfig.IsReadOnly;
    public bool IsReadOnly => !CanEditRules;

    public LintelConfigRuleViewModel SelectedConfig {
        get => _selectedConfig;
        set {
            RaiseAndSetIfChanged(ref _selectedConfig, value);
            RaisePropertyChanged(nameof(CanEditRules));
            RaisePropertyChanged(nameof(IsReadOnly));
            RefreshWalls();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ObservableCollection<string> LintelTypes {
        get => _lintelTypes;
        private set => RaiseAndSetIfChanged(ref _lintelTypes, value);
    }

    public LintelWallSettings[] GetWallSettings() {
        return Configs.SelectMany(config => config.PlaceRules.Select(rule => new LintelWallSettings {
            RuleConfigName = config.Name,
            LintelPlaceRuleName = rule.Name,
            WallTypeNames = rule.Walls.Where(wall => wall.IsChecked).Select(wall => wall.Name).ToArray()
        })).ToArray();
    }

    public string GetErrorText() {
        if(SelectedConfig is null) {
            return Text("Validation.SelectConfig");
        }
        string error = ValidateConfig(SelectedConfig);
        if(error is not null) {
            return error;
        }
        foreach(var config in Configs.Where(config => config != SelectedConfig && config.IsDirty)) {
            error = ValidateConfig(config);
            if(error is not null) {
                return config.Name + ": " + error;
            }
        }
        return null;
    }

    public IEnumerable<string> GetPendingPaths() {
        return Configs.Where(config => config.IsDirty || config.IsReadOnly).Select(config => config.Name)
            .Concat(_deletedConfigs).Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => _storage.GetProjectConfigRulePath(name)
                ?? throw new IOException(Text("Validation.RulesFolder")));
    }

    public void SaveConfigs() {
        foreach(var config in Configs.Where(config => config.IsDirty || config.IsReadOnly)) {
            string path = _storage.GetProjectConfigRulePath(config.Name)
                          ?? throw new IOException(Text("Validation.RulesFolder"));
            _serializer.SaveConfigRule(config.GetConfig(), path);
        }
        foreach(string name in _deletedConfigs.Where(name => !Configs.Any(config => SameName(config.Name, name)))) {
            string path = _storage.GetProjectConfigRulePath(name);
            if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    private LintelConfigRuleViewModel CreateConfigViewModel(LintelConfigRule source) {
        var config = new LintelConfigRuleViewModel {
            Name = source.RuleConfigName,
            IsReadOnly = SameName(source.RuleConfigName, _systemConfig.DefaultLintelConfigRuleName)
        };
        foreach(var sourceRule in source.LintelPlaceRules ?? []) {
            var rule = CreatePlaceRule(config, sourceRule?.LintelPlaceRuleName ?? string.Empty,
                sourceRule?.LintelRules ?? []);
            var names = (_settings.LintelWallSettings ?? []).Where(setting =>
                    SameName(setting.RuleConfigName, config.Name) && setting.LintelPlaceRuleName == rule.Name)
                .SelectMany(setting => setting.WallTypeNames ?? []).ToHashSet();
            foreach(string name in names.Except(rule.Walls.Select(wall => wall.Name))) {
                var missingWall = new WallTypeViewModel { Name = name, IsChecked = true };
                rule.Walls.Add(missingWall);
            }
            foreach(var wall in rule.Walls) {
                wall.IsChecked = names.Contains(wall.Name);
            }
            config.PlaceRules.Add(rule);
        }
        config.SelectedRule = config.PlaceRules.FirstOrDefault();
        config.IsDirty = false;
        return config;
    }

    private LintelPlaceRuleViewModel CreatePlaceRule(LintelConfigRuleViewModel config, string name,
        IEnumerable<LintelRule> rules) {
        var rule = new LintelPlaceRuleViewModel(name, rules, _repository.RevitWalls
            .OrderBy(wall => wall.WallType.Name).Select(wall => new WallTypeViewModel {
                Name = wall.WallType.Name, WallType = wall.WallType
            }), _commandFactory);
        rule.Changed += () => {
            if(_updating) {
                return;
            }
            if(!config.IsReadOnly) {
                config.IsDirty = true;
            }
            RefreshWalls();
            CommandManager.InvalidateRequerySuggested();
        };
        return rule;
    }

    private void RefreshLintelTypes() {
        var family = _familySettings.GetSelectedFamilies().FirstOrDefault();
        LintelTypes = new ObservableCollection<string>((family?.FamilySymbols ?? [])
            .Select(symbol => symbol.Name).OrderBy(name => name));
        CommandManager.InvalidateRequerySuggested();
    }

    // Обновляет доступность стен, сохраняя загруженные назначения при открытии настройки.
    private void RefreshWalls(string newlySelectedWall = null) {
        if(_updating) {
            return;
        }
        _updating = true;
        try {
            var structureNames = _generalSettings.GetStructureWallNames().ToHashSet();
            foreach(var config in Configs) {
                foreach(var rule in config.PlaceRules) {
                    var usedNames = config.PlaceRules.Where(other => other != rule)
                        .SelectMany(other => other.Walls.Where(wall => wall.IsChecked))
                        .Select(wall => wall.Name).ToHashSet();
                    foreach(var wall in rule.Walls) {
                        bool isStructural = structureNames.Contains(wall.Name);
                        if(newlySelectedWall is not null && wall.Name == newlySelectedWall) {
                            wall.IsChecked = false;
                        }
                        wall.IsAvailable = wall.IsChecked || (!isStructural && !usedNames.Contains(wall.Name));
                    }
                    rule.ApplySearchWalls();
                }
            }
        } finally {
            _updating = false;
        }
    }

    private string ValidateConfig(LintelConfigRuleViewModel config) {
        if(!LintelConfigRuleStorage.IsValidRuleName(config.Name) || config.PlaceRules.Count == 0) {
            return Text("Validation.ConfigRules");
        }
        var types = LintelTypes.ToHashSet();
        var structureNames = _generalSettings.GetStructureWallNames().ToHashSet();
        var usedWalls = new HashSet<string>();
        var ruleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string configError = null;
        foreach(var rule in config.PlaceRules) {
            string error = string.IsNullOrWhiteSpace(rule.Name) || !ruleNames.Add(rule.Name)
                ? Text("Validation.RuleName") : null;
            if(rule.Rules.Count == 0) {
                error ??= Text("Validation.EmptyRules");
            }
            int? previousMaxWidth = null;
            foreach(var row in rule.Rules) {
                string rowError;
                if(!row.TryGetRule(out var typedRule)) {
                    rowError = Text("Validation.RuleDimensions");
                    previousMaxWidth = null;
                } else {
                    string key = LintelRuleValidator.GetErrorKey(typedRule, previousMaxWidth, types);
                    rowError = key is null ? null : Text(key);
                    previousMaxWidth = typedRule.MaxWidth;
                }
                if(row.ErrorText != rowError) {
                    row.ErrorText = rowError;
                }
                error ??= rowError;
            }
            foreach(var wall in rule.Walls.Where(wall => wall.IsChecked)) {
                if(wall.WallType is null) {
                    error ??= Text("Validation.MissingWallType");
                }
                if(structureNames.Contains(wall.Name)) {
                    error ??= Text("Manager.Validation.StructureRuleConflict");
                }
                if(!usedWalls.Add(wall.Name)) {
                    error ??= Text("Validation.DuplicateWall");
                }
            }
            if(rule.ErrorText != error) {
                rule.ErrorText = error;
            }
            configError ??= error;
        }
        return configError;
    }

    private async Task CreateConfigAsync() {
        string name = await _dialogs.RequestNameAsync(Text("Config.Create"), UniqueName(Text("Config.NewName")),
            IsNewConfigName);
        if(name is null) {
            return;
        }
        var config = CreateConfigViewModel(new LintelConfigRule { RuleConfigName = name, LintelPlaceRules = [] });
        Configs.Add(config);
        SelectedConfig = config;
        AddPlaceRule(config, Text("LintelConfigRuleResolver.DefaultRuleWallName"));
    }

    private async Task ImportConfigAsync() {
        _openDialog.Filter = Text("Config.JsonFilter");
        _openDialog.Multiselect = false;
        if(!_openDialog.ShowDialog()) {
            return;
        }
        try {
            var source = _serializer.LoadConfigRule(_openDialog.File.FullName);
            if(source?.LintelPlaceRules is null) {
                throw new InvalidDataException(Text("Config.InvalidFile"));
            }
            string name = Path.GetFileNameWithoutExtension(_openDialog.File.FullName);
            if(!IsNewConfigName(name)) {
                name = await _dialogs.RequestNameAsync(Text("Config.Import"), UniqueName(name), IsNewConfigName);
            }
            if(name is null) {
                return;
            }
            source.RuleConfigName = name;
            var config = CreateConfigViewModel(source);
            config.IsDirty = true;
            Configs.Add(config);
            SelectedConfig = config;
        } catch(Exception ex) {
            ShowError(ex);
        }
    }

    private void SaveConfig() {
        var config = SelectedConfig;
        string error = ValidateConfig(config);
        if(error is not null) {
            ShowError(new InvalidDataException(error));
            return;
        }
        string path = _storage.GetProjectConfigRulePath(config.Name)
                      ?? throw new IOException(Text("Validation.RulesFolder"));
        _serializer.SaveConfigRule(config.GetConfig(), path);
        config.IsDirty = false;
    }

    private void SaveAsConfig() {
        string error = ValidateConfig(SelectedConfig);
        if(error is not null) {
            ShowError(new InvalidDataException(error));
            return;
        }
        _saveDialog.Filter = Text("Config.JsonFilter");
        _saveDialog.DefaultExt = ".json";
        _saveDialog.AddExtension = true;
        if(!_saveDialog.ShowDialog(_storage.ProjectConfigRuleDirectory, UniqueName(SelectedConfig.Name) + ".json")) {
            return;
        }
        try {
            string name = Path.GetFileNameWithoutExtension(_saveDialog.File.FullName);
            if(!LintelConfigRuleStorage.IsValidRuleName(name)
                || SameName(name, _systemConfig.DefaultLintelConfigRuleName)) {
                throw new InvalidDataException(Text("Dialog.InvalidName"));
            }
            var existing = Configs.FirstOrDefault(config => SameName(config.Name, name));
            if(existing is not null && existing != SelectedConfig && !Confirm(Text("Config.ReplaceConfirm"))) {
                return;
            }
            var source = SelectedConfig.GetConfig();
            source.RuleConfigName = name;
            _serializer.SaveConfigRule(source, _saveDialog.File.FullName);
            var newConfig = CloneConfig(SelectedConfig, source);
            if(existing is not null) {
                Configs.Remove(existing);
            }
            Configs.Add(newConfig);
            SelectedConfig = newConfig;
        } catch(Exception ex) {
            ShowError(ex);
        }
    }

    private void CopyConfig() {
        string projectName = _repository.Document.Title;
        foreach(char invalid in Path.GetInvalidFileNameChars()) {
            projectName = projectName.Replace(invalid, '_');
        }
        string baseName = SelectedConfig.Name + "_" + projectName;
        if(baseName.Length > 140) {
            baseName = baseName.Substring(0, 140).TrimEnd(' ', '.');
        }
        var source = SelectedConfig.GetConfig();
        source.RuleConfigName = UniqueName(baseName);
        var config = CloneConfig(SelectedConfig, source);
        Configs.Add(config);
        SelectedConfig = config;
    }

    private LintelConfigRuleViewModel CloneConfig(LintelConfigRuleViewModel original, LintelConfigRule source) {
        var config = CreateConfigViewModel(source);
        for(int index = 0; index < config.PlaceRules.Count; index++) {
            for(int rowIndex = 0; rowIndex < config.PlaceRules[index].Rules.Count; rowIndex++) {
                var originalRow = original.PlaceRules[index].Rules[rowIndex];
                var row = config.PlaceRules[index].Rules[rowIndex];
                row.MinWidthText = originalRow.MinWidthText;
                row.MaxWidthText = originalRow.MaxWidthText;
                row.WallSupportText = originalRow.WallSupportText;
                row.LintelType = originalRow.LintelType;
            }
            var names = original.PlaceRules[index].Walls.Where(wall => wall.IsChecked)
                .Select(wall => wall.Name).ToHashSet();
            foreach(string name in names.Except(config.PlaceRules[index].Walls.Select(wall => wall.Name))) {
                config.PlaceRules[index].Walls.Add(new WallTypeViewModel { Name = name, IsChecked = true });
            }
            foreach(var wall in config.PlaceRules[index].Walls) {
                wall.IsChecked = names.Contains(wall.Name);
            }
        }
        config.IsDirty = true;
        return config;
    }

    private void DeleteConfig() {
        if(!Confirm(Text("Config.DeleteConfirm") + Environment.NewLine + SelectedConfig.Name)) {
            return;
        }
        _deletedConfigs.Add(SelectedConfig.Name);
        Configs.Remove(SelectedConfig);
        SelectedConfig = Configs.FirstOrDefault();
    }

    private async Task AddPlaceRuleAsync() {
        var config = SelectedConfig;
        string name = await _dialogs.RequestNameAsync(Text("Rule.Create"),
            UniqueRuleName(config, Text("LintelConfigRuleResolver.DefaultRuleWallName")),
            value => IsNewRuleName(config, value, null));
        if(name is not null) {
            AddPlaceRule(config, name);
        }
    }

    private void AddPlaceRule(LintelConfigRuleViewModel config, string name) {
        var rule = CreatePlaceRule(config, name, [new LintelRule {
            MinWidth = (int) _systemConfig.DefaultMinWidthMm,
            MaxWidth = (int) _systemConfig.DefaultMaxWidthMm,
            WallSupport = (int) _systemConfig.DefaultWallSupportMm,
            LintelType = _systemConfig.DefaultLintelType
        }]);
        config.PlaceRules.Add(rule);
        config.SelectedRule = rule;
        config.IsDirty = true;
        RefreshWalls();
    }

    private async Task RenamePlaceRuleAsync(LintelPlaceRuleViewModel rule) {
        var config = SelectedConfig;
        string name = await _dialogs.RequestNameAsync(Text("Rule.Rename"), rule.Name,
            value => IsNewRuleName(config, value, rule));
        if(name is not null) {
            rule.Name = name;
            config.IsDirty = true;
        }
    }

    private void DeletePlaceRule(LintelPlaceRuleViewModel rule) {
        int index = SelectedConfig.PlaceRules.IndexOf(rule);
        bool isSelected = SelectedConfig.SelectedRule == rule;
        SelectedConfig.PlaceRules.Remove(rule);
        if(isSelected) {
            SelectedConfig.SelectedRule = SelectedConfig.PlaceRules
                .ElementAtOrDefault(Math.Min(index, SelectedConfig.PlaceRules.Count - 1));
        }
        SelectedConfig.IsDirty = true;
        RefreshWalls();
    }

    private bool CanAddRule() {
        return CanEditRules && SelectedConfig.SelectedRule is not null
            && (!int.TryParse(SelectedConfig.SelectedRule.Rules.LastOrDefault()?.MaxWidthText, out int max)
                || max < int.MaxValue);
    }

    private void AddRule() {
        const int rangeStepMm = 1000;
        var rules = SelectedConfig.SelectedRule.Rules;
        bool hasPreviousMaxWidth = int.TryParse(rules.LastOrDefault()?.MaxWidthText, out int previousMaxWidth);
        int min = hasPreviousMaxWidth ? previousMaxWidth + 1 : (int) _systemConfig.DefaultMinWidthMm;
        int max = hasPreviousMaxWidth
            ? (int) Math.Min((long) previousMaxWidth + rangeStepMm, int.MaxValue)
            : (int) _systemConfig.DefaultMaxWidthMm;
        rules.Add(new LintelRuleViewModel(new LintelRule {
            MinWidth = min,
            MaxWidth = max,
            WallSupport = (int) _systemConfig.DefaultWallSupportMm,
            LintelType = rules.LastOrDefault()?.LintelType ?? string.Empty
        }));
    }

    private void DeleteRule(LintelRuleViewModel rule) {
        SelectedConfig.SelectedRule?.Rules.Remove(rule);
    }

    private bool IsNewConfigName(string name) {
        return LintelConfigRuleStorage.IsValidRuleName(name)
               && !SameName(name, _systemConfig.DefaultLintelConfigRuleName)
               && !Configs.Any(config => SameName(config.Name, name));
    }

    private string UniqueName(string baseName) {
        foreach(char invalid in Path.GetInvalidFileNameChars()) {
            baseName = baseName.Replace(invalid, '_');
        }
        baseName = baseName.Trim().TrimEnd('.');
        if(baseName.Length > 125) {
            baseName = baseName.Substring(0, 125).TrimEnd(' ', '.');
        }
        if(!LintelConfigRuleStorage.IsValidRuleName(baseName)) {
            baseName = Text("Config.NewName");
        }
        string name = baseName;
        int index = 2;
        while(!IsNewConfigName(name)) {
            name = baseName + "_" + (index++).ToString(CultureInfo.InvariantCulture);
        }
        return name;
    }

    private static bool IsNewRuleName(LintelConfigRuleViewModel config, string name,
        LintelPlaceRuleViewModel current) {
        return !string.IsNullOrWhiteSpace(name) && !config.PlaceRules.Any(rule =>
            rule != current && SameName(rule.Name, name));
    }

    private static string UniqueRuleName(LintelConfigRuleViewModel config, string baseName) {
        string name = baseName;
        int index = 2;
        while(!IsNewRuleName(config, name, null)) {
            name = baseName + " " + (index++).ToString(CultureInfo.InvariantCulture);
        }
        return name;
    }

    private static bool SameName(string first, string second) {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }

    private string Text(string key) {
        return _localization.GetLocalizedString(key);
    }

    private bool Confirm(string message) {
        return _messages.Show(message, Text("MainWindow.Title"), MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private void ShowError(Exception exception) {
        _messages.Show(exception.Message, Text("Common.Error"), MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
