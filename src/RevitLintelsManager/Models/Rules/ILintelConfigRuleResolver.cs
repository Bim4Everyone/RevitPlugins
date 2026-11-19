using System.Collections.Generic;

namespace RevitLintelsManager.Models.Rules;

/// <summary>Контракт LintelConfigRuleResolver.</summary>
public interface ILintelConfigRuleResolver {
    IList<string> LoadErrors { get; }
    IList<LintelConfigRule> LintelConfigRules { get; }

    /// <summary>Находит конфигурацию по имени без учёта регистра.</summary>
    /// <param name="name">Имя конфигурации.</param>
    /// <returns>Правила или null.</returns>
    LintelConfigRule GetLintelConfigRule(string name);
}
