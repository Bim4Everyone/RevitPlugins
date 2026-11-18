using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;

using RevitLintelsManager.Models.Rules;

namespace RevitLintelsManager.Models.Configs;

internal class LintelManagerConfigValidator(RevitRepository repository) {
    public bool Validate(LintelManagerConfig config) {
        var family = config?.LintelFamilyConfig;
        if(family?.RevitLintelFamily is null || config.OpeningFamilyConfig is null
            || !config.OpeningFamilyConfig.RevitOpeningFamily.Any() || config.PhaseId is null
            || !ParamService.IsYesNo(config.LintelFixParam) || !config.MinimalHeightAboveOpeningMm.HasValue
            || double.IsNaN(config.MinimalHeightAboveOpeningMm.Value)
            || double.IsInfinity(config.MinimalHeightAboveOpeningMm.Value)
            || config.MinimalHeightAboveOpeningMm.Value < 0 || config.StructureWalls is null
            || !config.StructureWalls.Any()) {
            return false;
        }
        RevitParam[] parameters = [
            family.LintelWidth, family.LintelThickness, family.LintelRightOffset, family.LintelLeftOffset,
            family.LintelRightCorner, family.LintelLeftCorner, family.LintelRightWelding, family.LintelLeftWelding,
            config.OpeningFamilyConfig.LintelOpeningHeight, config.OpeningFamilyConfig.LintelOpeningWidth
        ];
        if(parameters.Any(parameter => parameter is null)) {
            return false;
        }
        var rules = config.LintelConfigRule?.LintelPlaceRules?.ToArray();
        if(rules is null || rules.Length == 0) {
            return false;
        }
        var types = family.RevitLintelFamily.FamilySymbols.Select(symbol => symbol.Name).ToHashSet();
        var wallNames = repository.RevitWalls.Select(wall => wall.WallType.Name).ToHashSet();
        var structureNames = config.StructureWalls.Select(wall => wall.WallType.Name).ToHashSet();
        var usedWalls = new HashSet<string>();
        var ruleNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        foreach(var rule in rules) {
            if(rule is null || string.IsNullOrWhiteSpace(rule.LintelPlaceRuleName)
                || !ruleNames.Add(rule.LintelPlaceRuleName) || rule.LintelRules is null) {
                return false;
            }
            var ranges = rule.LintelRules.ToArray();
            if(ranges.Length == 0) {
                return false;
            }
            int? previousMax = null;
            foreach(var range in ranges) {
                if(LintelRuleValidator.GetErrorKey(range, previousMax, types) is not null) {
                    return false;
                }
                previousMax = range.MaxWidth;
            }
            foreach(string name in rule.WallTypeNames ?? []) {
                if(!wallNames.Contains(name) || structureNames.Contains(name) || !usedWalls.Add(name)) {
                    return false;
                }
            }
        }
        return true;
    }
}
