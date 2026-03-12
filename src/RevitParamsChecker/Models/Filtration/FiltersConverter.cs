using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Filtration;

internal class FiltersConverter {
    private readonly IConfigSerializer _serializer;

    public FiltersConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Конвертирует массив фильтров в строку
    /// </summary>
    public string ConvertToString(Filter[] filters) {
        return _serializer.Serialize(filters);
    }

    /// <summary>
    /// Конвертирует строку в массив фильтров
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Исключение, если конвертировать данную строку нельзя</exception>
    public Filter[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Filter[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
