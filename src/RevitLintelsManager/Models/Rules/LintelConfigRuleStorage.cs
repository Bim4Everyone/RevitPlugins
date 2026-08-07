using System;
using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleStorage {
    private readonly SystemPluginConfig _systemPluginConfig;

    internal LintelConfigRuleStorage(SystemPluginConfig systemPluginConfig) {
        _systemPluginConfig = systemPluginConfig;
    }
    
    // Путь корпоративного конфига
    public string TemplateConfigRulePath => GetTemplateConfigRulePath();
    
    // Пути всех конфигов из папки пользователя
    public IList<string> ProjectConfigRulePaths => GetProjectConfigRulePaths();
    
    // Метод получения корпоративного конфига
    private string GetTemplateConfigRulePath() {
        string directory = Path.Combine(
            _systemPluginConfig.DefaultSettingPath,
            ModuleEnvironment.RevitVersion,
            nameof(RevitLintelsManager));
        
        string path = Path.Combine(
            directory,
            _systemPluginConfig.DefaultLintelConfigRuleName + ".json");
        
        if(!Directory.Exists(directory) || !File.Exists(path)) {
            return null;
        }
        
        return path;
    }

    // Метод получения всех конфигов из папки пользователя
    private IList<string> GetProjectConfigRulePaths() {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            _systemPluginConfig.DefaultSettingFolderName,
            ModuleEnvironment.RevitVersion,
            nameof(RevitLintelsManager),
            _systemPluginConfig.DefaultRuleFolderName);

        Directory.CreateDirectory(directory);

        return Directory.GetFiles(directory, "*.json");
    }
    
}
