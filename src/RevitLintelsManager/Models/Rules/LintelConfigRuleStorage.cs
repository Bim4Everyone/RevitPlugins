using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        if(!IsValidRuleName(lintelConfigRuleName)) {
            throw new ArgumentException(nameof(lintelConfigRuleName));
        }
        string directory = ProjectConfigRuleDirectory;
        return !Directory.Exists(directory) 
            ? null 
            : Path.Combine(directory, lintelConfigRuleName + ".json");
    }

    public static bool IsValidRuleName(string name) {
        if(string.IsNullOrWhiteSpace(name) || name.Length > 150 || name != name.Trim()
            || name.EndsWith(".", StringComparison.Ordinal)
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            return false;
        }
        string stem = name.Split('.')[0];
        string[] reservedNames = ["CON", "PRN", "AUX", "NUL", "CLOCK$"];
        return !reservedNames.Contains(stem, StringComparer.OrdinalIgnoreCase)
               && !Enumerable.Range(1, 9).Any(index =>
                   string.Equals(stem, "COM" + index, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(stem, "LPT" + index, StringComparison.OrdinalIgnoreCase));
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
        try {
            return !Directory.Exists(directory) ? [] : Directory.GetFiles(directory, "*.json");
        } catch(Exception ex) {
            _loggerService.Error(ex.Message);
            return [];
        }
    }
    
}
