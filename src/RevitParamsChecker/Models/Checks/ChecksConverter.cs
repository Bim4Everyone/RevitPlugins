using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Checks;

internal class ChecksConverter {
    private readonly IConfigSerializer _serializer;

    public ChecksConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string ConvertToString(Check[] filters) {
        return _serializer.Serialize(filters);
    }

    public Check[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Check[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
