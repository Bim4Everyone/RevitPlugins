using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

internal class LintelManagerConfigResolver(RevitRepository repository,
    ILintelConfigRuleResolver ruleResolver, IParamService paramService, SystemPluginConfig systemConfig)
    : ILintelManagerConfigResolver {
    /// <inheritdoc />
    public LintelManagerConfig BuildLintelManagerConfig(LintelManagerSettings settings) {
        if(settings is null) {
            return null;
        }
        var concreteTypes = settings.GetReinforcedConcreteTypes().ToArray();
        var steelTypes = (settings.SteelTypes ?? []).ToArray();
        var catalog = concreteTypes.Length + steelTypes.Length == 0 ? [] : repository.GetStructureTypes();
        var typesByKey = catalog.ToLookup(type => type.Settings.GetKey());
        var concrete = ResolveStructureTypes(concreteTypes, systemConfig.ReinforcedConcreteCategories, typesByKey);
        var steel = ResolveStructureTypes(steelTypes, systemConfig.SteelCategories, typesByKey);
        var structuralWallIds = concrete.Where(type => type.Settings?.IsWallType() == true)
            .Select(type => type.UniqueId).Where(id => id is not null).ToHashSet();
        var lintelFamily = BuildLintelFamilyConfig(settings.LintelFamilySettings);
        var openingFamilies = BuildOpeningFamilyConfig(settings.OpeningFamilySettings);
        var source = ruleResolver.GetLintelConfigRule(settings.LintelConfigRuleName);
        var config = source is null ? null : new LintelConfigRule {
            RuleConfigName = source.RuleConfigName,
            LintelPlaceRules = source.LintelPlaceRules?.Select(rule => new LintelPlaceRule {
                LintelPlaceRuleName = rule?.LintelPlaceRuleName,
                LintelRules = rule?.LintelRules?.Select(range => range is null ? null : new LintelRule {
                    MinWidth = range.MinWidth, MaxWidth = range.MaxWidth,
                    WallSupport = range.WallSupport, LintelType = range.LintelType
                }).ToArray(),
                WallTypeNames = (settings.LintelWallSettings ?? []).Where(wallSettings =>
                    wallSettings is not null && string.Equals(wallSettings.RuleConfigName, source.RuleConfigName,
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
            MinimalBearingMm = settings.MinimalBearingMm ?? systemConfig.DefaultMinimalBearingMm,
            ReinforcedConcreteTypes = concrete,
            SteelTypes = steel,
            StructureWalls = structuralWallIds.Count == 0 ? []
                : repository.RevitWalls.Where(wall => structuralWallIds.Contains(wall.WallType.UniqueId)).ToArray(),
            PhaseId = repository.GetPhaseIdByName(settings.PhaseName)
        };
    }

    /// <inheritdoc />
    public ManagerConfigSnapshot CreateSnapshot(LintelManagerConfig config) {
        var family = config.LintelFamilyConfig;
        var opening = config.OpeningFamilyConfig;
        var wallTypes = repository.RevitWalls.Select(wall => wall.WallType)
            .ToLookup(wall => wall.Name, wall => new ConfigElementKey(wall.UniqueId, wall.Name));
        var parameters = new Dictionary<string, string> {
            [nameof(family.LintelWidth)] = family.LintelWidth.Name,
            [nameof(family.LintelThickness)] = family.LintelThickness.Name,
            [nameof(family.LintelRightOffset)] = family.LintelRightOffset.Name,
            [nameof(family.LintelLeftOffset)] = family.LintelLeftOffset.Name,
            [nameof(family.LintelRightCorner)] = family.LintelRightCorner.Name,
            [nameof(family.LintelLeftCorner)] = family.LintelLeftCorner.Name,
            [nameof(family.LintelRightWelding)] = family.LintelRightWelding.Name,
            [nameof(family.LintelLeftWelding)] = family.LintelLeftWelding.Name,
            [nameof(opening.LintelOpeningHeight)] = opening.LintelOpeningHeight.Name,
            [nameof(opening.LintelOpeningWidth)] = opening.LintelOpeningWidth.Name,
            [nameof(config.LintelFixParam)] = config.LintelFixParam.Name
        };
        var phase = repository.Document.GetElement(config.PhaseId);
        return new ManagerConfigSnapshot(
            new ConfigElementKey(family.RevitLintelFamily.Family.UniqueId, family.RevitLintelFamily.Family.Name),
            family.RevitLintelFamily.FamilySymbols.Select(type => new ConfigElementKey(type.UniqueId, type.Name)),
            opening.RevitOpeningFamily.Select(item => new ConfigElementKey(item.Family.UniqueId, item.Family.Name)),
            config.StructureWalls.Select(wall => new ConfigElementKey(wall.WallType.UniqueId, wall.WallType.Name)),
            new ConfigElementKey(phase.UniqueId, phase.Name), parameters, config.LintelConfigRule.RuleConfigName,
            config.LintelConfigRule.LintelPlaceRules.Select(rule => new PlacementRuleSnapshot(
                rule.LintelPlaceRuleName, (rule.WallTypeNames ?? []).Select(name => wallTypes[name].Single()),
                rule.LintelRules.Select(range => new LintelRuleSnapshot(range.MinWidth, range.MaxWidth,
                    range.WallSupport, range.LintelType)))), config.MinimalHeightAboveOpeningMm.Value,
            config.MinimalBearingMm, config.ReinforcedConcreteTypes, config.SteelTypes);
    }

    // Разрешает семейство перемычки и сопоставленные параметры.
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

    // Разрешает все выбранные семейства проёмов.
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

    // Разрешает составные ключи только в допустимых категориях, сохраняя каждую исходную запись.
    private static IReadOnlyList<StructureTypeSelection> ResolveStructureTypes(
        IEnumerable<StructureTypeSettings> settings, IEnumerable<BuiltInCategory> categories,
        ILookup<(string Category, string FamilyName, string TypeName), StructureTypeSelection> catalog) {
        var allowed = categories.Select(category => category.ToString()).ToHashSet();
        var selections = new List<StructureTypeSelection>();
        foreach(var type in settings) {
            if(type is null || string.IsNullOrWhiteSpace(type.Category) || string.IsNullOrWhiteSpace(type.TypeName)
                || (type.FamilyName is not null && string.IsNullOrWhiteSpace(type.FamilyName))) {
                selections.Add(new StructureTypeSelection(type, StructureTypeResolution.Invalid));
                continue;
            }
            if(!allowed.Contains(type.Category)) {
                selections.Add(new StructureTypeSelection(type, StructureTypeResolution.UnsupportedCategory));
                continue;
            }
            var matches = catalog[type.GetKey()].Take(2).ToArray();
            selections.Add(matches.Length switch {
                0 => new StructureTypeSelection(type, StructureTypeResolution.Missing),
                1 => new StructureTypeSelection(type, StructureTypeResolution.Resolved, matches[0].UniqueId),
                _ => new StructureTypeSelection(type, StructureTypeResolution.Ambiguous)
            });
        }
        return selections.AsReadOnly();
    }
}
