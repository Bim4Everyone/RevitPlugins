using System.Collections.Generic;

namespace RevitLintelsManager.Models.Rules;

/// <summary>Контракт LintelConfigRuleStorage.</summary>
public interface ILintelConfigRuleStorage {
    string ProjectConfigRuleDirectory { get; }
    string TemplateConfigRulePath { get; }
    IList<string> ProjectConfigRulePaths { get; }

    /// <summary>Получает путь пользовательской конфигурации.</summary>
    /// <param name="lintelConfigRuleName">Допустимое имя конфигурации.</param>
    /// <returns>Путь файла или null.</returns>
    /// <exception cref="System.ArgumentException">Имя недопустимо.</exception>
    string GetProjectConfigRulePath(string lintelConfigRuleName);
}
