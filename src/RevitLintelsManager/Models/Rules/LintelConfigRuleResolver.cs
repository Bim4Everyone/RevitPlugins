using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleResolver : ILintelConfigRuleResolver {
    private readonly ILoggerService _loggerService;
    private readonly ILintelConfigRuleStorage _storage;
    private readonly ILintelConfigRuleSerializer _serializer;
    private readonly SystemPluginConfig _systemPluginConfig;
    private IList<LintelConfigRule> _configs;

    /// <summary>Создаёт LintelConfigRuleResolver с переданными зависимостями.</summary>
    public LintelConfigRuleResolver(ILoggerService loggerService, ILintelConfigRuleStorage storage,
        ILintelConfigRuleSerializer serializer, SystemPluginConfig systemPluginConfig) {
        _loggerService = loggerService;
        _storage = storage;
        _serializer = serializer;
        _systemPluginConfig = systemPluginConfig;
    }

    public IList<string> LoadErrors { get; } = new List<string>();
    public IList<LintelConfigRule> LintelConfigRules => _configs ??= LoadConfigs();

    /// <inheritdoc />
    public LintelConfigRule GetLintelConfigRule(string name) {
        return LintelConfigRules.FirstOrDefault(config =>
            string.Equals(config.RuleConfigName, name, StringComparison.OrdinalIgnoreCase));
    }

    // Загружает пользовательские правила и корпоративный шаблон.
    private IList<LintelConfigRule> LoadConfigs() {
        var configs = new List<LintelConfigRule>();
        foreach(string path in _storage.ProjectConfigRulePaths) {
            var config = LoadConfig(path);
            if(config is not null && !configs.Any(item => string.Equals(
                item.RuleConfigName, config.RuleConfigName, StringComparison.OrdinalIgnoreCase))) {
                configs.Add(config);
            }
        }
        var template = LoadConfig(_storage.TemplateConfigRulePath);
        if(template is not null) {
            template.RuleConfigName = _systemPluginConfig.DefaultLintelConfigRuleName;
            configs.RemoveAll(config => string.Equals(config.RuleConfigName, template.RuleConfigName,
                StringComparison.OrdinalIgnoreCase));
            configs.Insert(0, template);
        }
        return configs;
    }

    // Читает один файл и фиксирует ошибку загрузки.
    private LintelConfigRule LoadConfig(string path) {
        if(string.IsNullOrEmpty(path)) {
            return null;
        }
        try {
            var config = _serializer.LoadConfigRule(path);
            if(config?.LintelPlaceRules is null) {
                throw new InvalidDataException(path);
            }
            config.RuleConfigName = Path.GetFileNameWithoutExtension(path);
            return config;
        } catch(Exception ex) {
            _loggerService.Error(ex.Message);
            LoadErrors.Add(path);
            return null;
        }
    }
}
