using System.Collections.Generic;

namespace RevitLintelsManager.Models.Rules;

internal static class LintelRuleValidator {
    public static string GetErrorKey(LintelRule rule, int? previousMaxWidth, ISet<string> lintelTypes) {
        if(rule is null || rule.MinWidth <= 0 || rule.MaxWidth < rule.MinWidth || rule.WallSupport <= 0) {
            return "Validation.RuleDimensions";
        }
        if(previousMaxWidth.HasValue && (long) previousMaxWidth.Value + 1 != rule.MinWidth) {
            return "Validation.RuleSequence";
        }
        return string.IsNullOrWhiteSpace(rule.LintelType) || !lintelTypes.Contains(rule.LintelType)
            ? "Validation.MissingLintelType" : null;
    }
}
