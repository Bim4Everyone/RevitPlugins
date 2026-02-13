using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Checks;

internal class ChecksConverter {
    private readonly IConfigSerializer _serializer;

    public ChecksConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Конвертирует массив проверок в строку
    /// </summary>
    public string ConvertToString(Check[] checks) {
        return _serializer.Serialize(checks);
    }

    /// <summary>
    /// Конвертирует строку в массив проверок
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Исключение, если конвертировать данную строку нельзя</exception>
    public Check[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Check[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
