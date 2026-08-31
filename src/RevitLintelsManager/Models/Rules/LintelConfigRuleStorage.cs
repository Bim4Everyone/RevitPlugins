using System;
using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleStorage {
    private readonly ILoggerService _loggerService;
    private readonly SystemPluginConfig _systemPluginConfig;

    public LintelConfigRuleStorage(ILoggerService loggerService, SystemPluginConfig systemPluginConfig) {
        _loggerService = loggerService;
        _systemPluginConfig = systemPluginConfig;
    }
    
    // Путь папки пользовательских конфигов
    public string ProjectConfigRuleDirectory => GetProjectConfigRuleDirectory();
    
    // Путь корпоративного конфига
    public string TemplateConfigRulePath => GetTemplateConfigRulePath();
    
    // Пути всех конфигов из папки пользователя
    public IList<string> ProjectConfigRulePaths => GetProjectConfigRulePaths();

    // Метод получения пути сохраняемого пользовательского конфига
    public string GetProjectConfigRulePath(string lintelConfigRuleName) {
        string directory = ProjectConfigRuleDirectory;
        return !Directory.Exists(directory) 
            ? null 
            : Path.Combine(directory, lintelConfigRuleName + ".json");
    }

    
    // Метод получения папки пользовательских конфигов
    private string GetProjectConfigRuleDirectory() {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            _systemPluginConfig.DefaultSettingFolderName,
            ModuleEnvironment.RevitVersion,
            nameof(RevitLintelsManager),
            _systemPluginConfig.DefaultRuleFolderName);
        
        try {
            Directory.CreateDirectory(directory);
            return directory;
        } catch(Exception ex) {
            _loggerService.Error(ex.Message);
            return null;
        }
    }
   
    
    // Метод получения корпоративного конфига
    private string GetTemplateConfigRulePath() {
        string directory = Path.Combine(
            _systemPluginConfig.DefaultSettingPath,
            ModuleEnvironment.RevitVersion,
            nameof(RevitLintelsManager));
        
        string path = Path.Combine(
            directory,
            _systemPluginConfig.DefaultLintelConfigRuleName + ".json");
        
        if(Directory.Exists(directory) && File.Exists(path)) {
            return path;
        }
        
        return null;
    }

    // Метод получения всех конфигов из папки пользователя
    private IList<string> GetProjectConfigRulePaths() {
        string directory = ProjectConfigRuleDirectory;
        return !Directory.Exists(directory) 
            ? [] 
            : Directory.GetFiles(directory, "*.json");
    }
    
}
