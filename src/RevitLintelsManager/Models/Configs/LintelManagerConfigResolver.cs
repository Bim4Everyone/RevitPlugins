using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

internal class LintelManagerConfigResolver(RevitRepository repository,
    LintelConfigRuleResolver ruleResolver, ParamService paramService) {
    public LintelManagerConfig BuildLintelManagerConfig(LintelManagerSettings settings) {
        if(settings is null) {
            return null;
        }
        var lintelFamily = BuildLintelFamilyConfig(settings.LintelFamilySettings);
        var openingFamilies = BuildOpeningFamilyConfig(settings.OpeningFamilySettings);
        var source = ruleResolver.GetLintelConfigRule(settings.LintelConfigRuleName);
        var config = source is null ? null : new LintelConfigRule {
            RuleConfigName = source.RuleConfigName,
            LintelPlaceRules = source.LintelPlaceRules?.Select(rule => new LintelPlaceRule {
                LintelPlaceRuleName = rule?.LintelPlaceRuleName,
                LintelRules = rule?.LintelRules,
                WallTypeNames = (settings.LintelWallSettings ?? []).Where(wallSettings =>
                    string.Equals(wallSettings.RuleConfigName, source.RuleConfigName,
                        StringComparison.OrdinalIgnoreCase)
                    && wallSettings.LintelPlaceRuleName == rule?.LintelPlaceRuleName)
                    .SelectMany(wallSettings => wallSettings.WallTypeNames ?? []).ToArray()
            }).ToArray()
        };
        return new LintelManagerConfig {
            LintelFamilyConfig = lintelFamily,
            OpeningFamilyConfig = openingFamilies,
            LintelConfigRule = config,
            LintelFixParam = paramService.GetFixParam(lintelFamily, openingFamilies, settings.LintelFixParamName),
            MinimalHeightAboveOpeningMm = settings.MinimalHeightAboveOpeningMm,
            StructureWalls = GetStructureWalls(settings.StructureWallTypeNames),
            PhaseId = repository.GetPhaseIdByName(settings.PhaseName)
        };
    }

    private LintelFamilyConfig BuildLintelFamilyConfig(LintelFamilySettings settings) {
        if(settings is null) {
            return null;
        }
        var family = repository.LintelFamilies.FirstOrDefault(item => item.Family.Name == settings.LintelFamily);
        return family is null ? null : new LintelFamilyConfig {
            RevitLintelFamily = family,
            LintelWidth = paramService.GetLintelParam(family, settings.LintelWidth, StorageType.Double),
            LintelThickness = paramService.GetLintelParam(family, settings.LintelThickness, StorageType.Double),
            LintelRightOffset = paramService.GetLintelParam(family, settings.LintelRightOffset, StorageType.Double),
            LintelLeftOffset = paramService.GetLintelParam(family, settings.LintelLeftOffset, StorageType.Double),
            LintelRightCorner = paramService.GetLintelParam(family, settings.LintelRightCorner, StorageType.Integer),
            LintelLeftCorner = paramService.GetLintelParam(family, settings.LintelLeftCorner, StorageType.Integer),
            LintelRightWelding = paramService.GetLintelParam(family, settings.LintelRightWelding, StorageType.Integer),
            LintelLeftWelding = paramService.GetLintelParam(family, settings.LintelLeftWelding, StorageType.Integer)
        };
    }

    private OpeningFamilyConfig BuildOpeningFamilyConfig(OpeningFamilySettings settings) {
        var names = (settings?.FamilyNames ?? []).ToHashSet();
        var families = repository.OpeningFamilies.Where(family => names.Contains(family.Family.Name)).ToArray();
        if(names.Count == 0 || families.Length != names.Count) {
            return null;
        }
        return new OpeningFamilyConfig {
            RevitOpeningFamily = families,
            LintelOpeningHeight = paramService.GetOpeningParam(families, settings.LintelOpeningHeight,
                StorageType.Double),
            LintelOpeningWidth = paramService.GetOpeningParam(families, settings.LintelOpeningWidth, StorageType.Double)
        };
    }

    private IEnumerable<RevitWall> GetStructureWalls(IEnumerable<string> wallTypeNames) {
        var names = (wallTypeNames ?? []).ToHashSet();
        var walls = repository.RevitWalls.Where(wall => names.Contains(wall.WallType.Name)).ToArray();
        return names.Count == walls.Length ? walls : null;
    }
}
