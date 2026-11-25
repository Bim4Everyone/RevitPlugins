using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Подбирает группу и диапазон для измерения контактов и расчёта перемычки.</summary>
internal sealed class PlacementRuleLookup(ManagerConfigSnapshot config) {
    private static readonly ConditionalWeakTable<ManagerConfigSnapshot, PlacementRuleLookup> _lookups = new();
    private readonly ILookup<string, PlacementRuleSnapshot> _groups = config.Rules.Where(group => group is not null)
        .SelectMany(group => group.WallTypes.Select(type => type?.UniqueId).Distinct().Select(id => (id, group)))
        .ToLookup(item => item.id, item => item.group, StringComparer.Ordinal);

    /// <summary>Переиспользует индекс назначений в пределах одного неизменяемого снимка.</summary>
    public static PlacementRuleLookup For(ManagerConfigSnapshot config) {
        return _lookups.GetValue(config, snapshot => new PlacementRuleLookup(snapshot));
    }

    /// <summary>Проверяет, что тип стены назначен хотя бы одной группе правил.</summary>
    public bool HasWallType(string wallTypeId) {
        return !string.IsNullOrEmpty(wallTypeId) && _groups.Contains(wallTypeId);
    }

    /// <summary>Ищет одну назначенную группу и один включённый диапазон.</summary>
    public (PlacementRuleSnapshot Group, LintelRuleSnapshot Rule, double? WidthMm, CalculationIssueCode? Issue)
        Resolve(string wallTypeId, double? widthMm) {
        double? rounded = CalculationNumbers.RoundDimension(widthMm);
        if(string.IsNullOrEmpty(wallTypeId)) {
            return (null, null, rounded, CalculationIssueCode.WallTypeUnresolved);
        }
        var groups = _groups[wallTypeId].Take(2).ToArray();
        if(groups.Length != 1) {
            return (null, null, rounded, groups.Length == 0 ? CalculationIssueCode.NoPlacementGroup
                : CalculationIssueCode.ConflictingPlacementGroups);
        }
        var group = groups[0];
        if(!rounded.HasValue) {
            return (group, null, null, CalculationIssueCode.InvalidDimensions);
        }
        var ranges = group.Rules.Where(rule => rule is not null && rule.MinWidth <= rounded && rounded <= rule.MaxWidth)
            .Take(2).ToArray();
        if(ranges.Length > 1) {
            return (group, null, rounded, CalculationIssueCode.ConflictingRules);
        }
        if(group.Rules.Count == 0 || group.Rules.Any(rule =>
            rule is null || rule.MinWidth <= 0 || rule.MaxWidth < rule.MinWidth || rule.WallSupport <= 0)) {
            return (group, null, rounded, CalculationIssueCode.InvalidConfiguration);
        }
        return ranges.Length == 1 ? (group, ranges[0], rounded, null)
            : (group, null, rounded, CalculationIssueCode.NoWidthRule);
    }
}
