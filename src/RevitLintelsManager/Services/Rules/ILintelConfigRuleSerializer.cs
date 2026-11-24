namespace RevitLintelsManager.Models.Rules;

/// <summary>Контракт LintelConfigRuleSerializer.</summary>
public interface ILintelConfigRuleSerializer {

    /// <summary>Читает конфигурацию правил из JSON.</summary>
    /// <param name="configPath">Путь файла.</param>
    /// <returns>Правила или null при отсутствии файла.</returns>
    LintelConfigRule LoadConfigRule(string configPath);

    /// <summary>Атомарно сохраняет конфигурацию правил.</summary>
    /// <param name="lintelConfigRule">Сохраняемые правила.</param>
    /// <param name="configPath">Путь файла.</param>
    /// <exception cref="System.ArgumentException">Путь пуст или конфигурация не задана.</exception>
    void SaveConfigRule(LintelConfigRule lintelConfigRule, string configPath);
}
