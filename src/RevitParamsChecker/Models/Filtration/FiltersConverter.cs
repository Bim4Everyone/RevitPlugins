using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Filtration;

internal class FiltersConverter {
    private readonly IConfigSerializer _serializer;

    public FiltersConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string ConvertToString(Filter[] filters) {
        return _serializer.Serialize(filters);
    }

    public Filter[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Filter[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
