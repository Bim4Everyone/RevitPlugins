using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Rules;

internal class RulesConverter {
    private readonly IConfigSerializer _serializer;

    public RulesConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Конвертирует массив правил в строку
    /// </summary>
    public string ConvertToString(Rule[] rules) {
        return _serializer.Serialize(rules);
    }

    /// <summary>
    /// Конвертирует строку в массив правил
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Исключение, если конвертировать данную строку нельзя</exception>
    public Rule[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Rule[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
