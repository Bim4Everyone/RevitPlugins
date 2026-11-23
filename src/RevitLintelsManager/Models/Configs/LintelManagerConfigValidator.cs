using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;

using RevitLintelsManager.Models.Rules;

namespace RevitLintelsManager.Models.Configs;

internal class LintelManagerConfigValidator(RevitRepository repository) : ILintelManagerConfigValidator {
    /// <inheritdoc />
    public IReadOnlyList<string> ValidateDetailed(LintelManagerConfig config) {
        if(config is null) {
            return new[] { "Manager.Validation.SettingsMissing" };
        }
        var family = config.LintelFamilyConfig;
        var errors = new List<string>();
        if(family?.RevitLintelFamily is null) {
            errors.Add("Manager.Validation.LintelFamily");
        }
        if(config.OpeningFamilyConfig?.RevitOpeningFamily?.Any() != true) {
            errors.Add("Manager.Validation.OpeningFamilies");
        }
        if(config.PhaseId is null) {
            errors.Add("Validation.SelectPhase");
        }
        if(!ParamService.IsYesNo(config.LintelFixParam)) {
            errors.Add("Validation.SelectFixParameter");
        }
        if(!config.MinimalHeightAboveOpeningMm.HasValue
            || double.IsNaN(config.MinimalHeightAboveOpeningMm.Value)
            || double.IsInfinity(config.MinimalHeightAboveOpeningMm.Value)
            || config.MinimalHeightAboveOpeningMm.Value < 0) {
            errors.Add("Validation.MinimalHeight");
        }
        if(double.IsNaN(config.MinimalBearingMm) || double.IsInfinity(config.MinimalBearingMm)
            || config.MinimalBearingMm < 1) {
            errors.Add("Manager.Validation.MinimalBearing");
        }
        var concreteKeys = ValidateStructureTypes(config.ReinforcedConcreteTypes, errors);
        var steelKeys = ValidateStructureTypes(config.SteelTypes, errors);
        if(concreteKeys.Overlaps(steelKeys)) {
            errors.Add("Manager.Validation.StructureTypeConflict");
        }
        RevitParam[] parameters = [
            family?.LintelWidth, family?.LintelThickness, family?.LintelRightOffset, family?.LintelLeftOffset,
            family?.LintelRightCorner, family?.LintelLeftCorner, family?.LintelRightWelding, family?.LintelLeftWelding,
            config.OpeningFamilyConfig?.LintelOpeningHeight, config.OpeningFamilyConfig?.LintelOpeningWidth
        ];
        if(parameters.Any(parameter => parameter is null)) {
            errors.Add("Manager.Validation.Parameters");
        }
        var rules = config.LintelConfigRule?.LintelPlaceRules?.ToArray();
        if(rules is null || rules.Length == 0) {
            errors.Add("Manager.Validation.Rules");
            return errors.Distinct().ToList().AsReadOnly();
        }
        var types = (family?.RevitLintelFamily?.FamilySymbols ?? []).Select(symbol => symbol.Name).ToHashSet();
        var wallNames = repository.RevitWalls.ToLookup(wall => wall.WallType.Name);
        var structureNames = (config.StructureWalls ?? []).Select(wall => wall.WallType.Name).ToHashSet();
        var usedWalls = new HashSet<string>();
        var ruleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var rule in rules) {
            if(rule is null || string.IsNullOrWhiteSpace(rule.LintelPlaceRuleName)
                || !ruleNames.Add(rule.LintelPlaceRuleName) || rule.LintelRules?.Any() != true) {
                errors.Add("Manager.Validation.Rules");
                continue;
            }
            int? previousMax = null;
            foreach(var range in rule.LintelRules) {
                string error = LintelRuleValidator.GetErrorKey(range, previousMax, types);
                if(error is not null) {
                    errors.Add(error);
                }
                previousMax = range?.MaxWidth;
            }
            foreach(string name in rule.WallTypeNames ?? []) {
                int matches = wallNames[name].Count();
                if(matches == 0) {
                    errors.Add("Validation.MissingWallType");
                } else if(matches > 1) {
                    errors.Add("Manager.Validation.AmbiguousWallType");
                } else if(structureNames.Contains(name)) {
                    errors.Add("Manager.Validation.StructureRuleConflict");
                } else if(!usedWalls.Add(name)) {
                    errors.Add("Validation.DuplicateWall");
                }
            }
        }
        return errors.Distinct().ToList().AsReadOnly();
    }

    // Проверяет сохранённые ключи одного перечня независимо от другого.
    private static HashSet<(string Category, string FamilyName, string TypeName)> ValidateStructureTypes(
        IEnumerable<StructureTypeSelection> selections, ICollection<string> errors) {
        var keys = new HashSet<(string Category, string FamilyName, string TypeName)>();
        foreach(var selection in selections) {
            if(selection.Settings is not null && !keys.Add(selection.Settings.GetKey())) {
                errors.Add("Manager.Validation.DuplicateStructureType");
            }
            string error = selection.Resolution switch {
                StructureTypeResolution.Resolved => null,
                StructureTypeResolution.Missing => "Manager.Validation.MissingStructureType",
                StructureTypeResolution.Ambiguous => "Manager.Validation.AmbiguousStructureType",
                StructureTypeResolution.UnsupportedCategory => "Manager.Validation.StructureTypeCategory",
                _ => "Manager.Validation.InvalidStructureType"
            };
            if(error is not null) {
                errors.Add(error);
            }
        }
        return keys;
    }
}
