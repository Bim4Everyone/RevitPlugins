using System;
using System.Linq;

namespace RevitLintelsManager.Models.Rules;

internal class LintelConfigRuleValidator {
    
    public bool Validate(LintelConfigRule templateConfigRule, LintelConfigRule configRule) {
        if(templateConfigRule is null || configRule is null) {
            return false;
        }

        if(templateConfigRule.LintelPlaceRules is null || configRule.LintelPlaceRules is null) {
            return false;
        }

        var templatePlaceRules = templateConfigRule.LintelPlaceRules.ToArray();
        var configPlaceRules = configRule.LintelPlaceRules.ToArray();
        
        if(templatePlaceRules.Length != configPlaceRules.Length) {
            return false;
        }
        
        foreach(var templatePlaceRule in templatePlaceRules) {
            var configPlaceRule = configPlaceRules.FirstOrDefault(
                rule => string.Equals(rule.LintelPlaceRuleName, templatePlaceRule.LintelPlaceRuleName, StringComparison.Ordinal));

            if(configPlaceRule is null) {
                return false;
            }

            if(!ValidateLintelRules(templatePlaceRule, configPlaceRule)) {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateLintelRules(LintelPlaceRule templatePlaceRule, LintelPlaceRule configPlaceRule) {

        if(templatePlaceRule.LintelRules is null || configPlaceRule.LintelRules is null) {
            return templatePlaceRule.LintelRules is null && configPlaceRule.LintelRules is null;
        }

        var templateRules = templatePlaceRule.LintelRules.ToArray();
        var configRules = configPlaceRule.LintelRules.ToArray();

        if(templateRules.Length != configRules.Length) {
            return false;
        }
        
        bool[] usedRules = new bool[configRules.Length];
        
        foreach(var templateRule in templateRules) {
            bool found = false;

            for(int i = 0; i < configRules.Length; i++) {
                if(usedRules[i]) {
                    continue;
                }

                if(!AreEqual(templateRule, configRules[i])) {
                    continue;
                }

                usedRules[i] = true;
                found = true;
                break;
            }

            if(!found) {
                return false;
            }
        }

        return true;
    }

    private static bool AreEqual(LintelRule templateRule, LintelRule configRule) {
        if(templateRule is null || configRule is null) {
            return templateRule is null && configRule is null;
        }

        return templateRule.MinWidth == configRule.MinWidth
               && templateRule.MaxWidth == configRule.MaxWidth
               && templateRule.WallSupport == configRule.WallSupport
               && string.Equals(templateRule.LintelType, configRule.LintelType, StringComparison.Ordinal);
    }
}
