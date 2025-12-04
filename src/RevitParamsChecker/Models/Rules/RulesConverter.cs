using System;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitParamsChecker.Models.Rules;

internal class RulesConverter {
    private readonly IConfigSerializer _serializer;

    public RulesConverter(IConfigSerializer serializer) {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string ConvertToString(Rule[] filters) {
        return _serializer.Serialize(filters);
    }

    public Rule[] ConvertFromString(string str) {
        try {
            return _serializer.Deserialize<Rule[]>(str);
        } catch(Exception) {
            throw new InvalidOperationException();
        }
    }
}
