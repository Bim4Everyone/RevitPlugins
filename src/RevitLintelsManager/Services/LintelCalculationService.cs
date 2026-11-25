using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <inheritdoc />
internal sealed class LintelCalculationService : ILintelCalculationService {
    /// <inheritdoc />
    public LintelCalculation Calculate(ManagerOpeningSnapshot opening, ManagerWallSnapshot wall,
        AboveOpeningFacts above, OpeningBearingFacts sides, LintelFamilyFacts family, ManagerConfigSnapshot config,
        string matchedLintelId = null) {
        var issues = new List<CalculationIssue>();
        string id = opening.Element.UniqueId;
        if(wall?.UniqueId != opening.CutWallUniqueId) {
            wall = null;
        }
        var (group, rule, width, ruleIssue) = PlacementRuleLookup.For(config)
            .Resolve(wall?.TypeUniqueId, opening.WidthMm);
        if(ruleIssue.HasValue) {
            issues.Add(new CalculationIssue(ruleIssue.Value));
        }
        double? thickness = CalculationNumbers.RoundDimension(wall?.WidthMm);
        if(!thickness.HasValue) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InvalidDimensions,
                detail: nameof(ManagerWallSnapshot.WidthMm)));
        }
        if(opening.HeightMm is not double height || !CalculationNumbers.IsFinite(height) || height <= 0) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InvalidDimensions,
                detail: nameof(ManagerOpeningSnapshot.HeightMm)));
        }

        if(above?.OpeningId != id) {
            above = null;
        }
        CheckCentralMasonry(above?.Masonry, config.MinimalHeightAboveOpeningMm, issues);
        if(sides?.OpeningId != id) {
            sides = null;
        }
        double minimum = config.MinimalBearingMm;
        bool validMinimum = LintelManagerConfigValidator.IsValidMinimalBearing(minimum);
        if(!validMinimum) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InvalidConfiguration,
                detail: nameof(config.MinimalBearingMm)));
        }
        var left = validMinimum && rule is not null
            ? CalculateBearing(BearingSide.Left, sides?.Left, rule.WallSupport, minimum, issues) : null;
        var right = validMinimum && rule is not null
            ? CalculateBearing(BearingSide.Right, sides?.Right, rule.WallSupport, minimum, issues) : null;

        bool familyIdentified = family?.FamilyId is not null && family.FamilyId == config.LintelFamily?.UniqueId;
        var type = rule is null ? null : ResolveType(rule, config, familyIdentified ? family : null, issues);
        var target = CalculateTarget(opening.Geometry, familyIdentified ? family.FamilyLeftSign : null, issues);
        var names = CheckParameters(config, familyIdentified ? family : null, issues);
        var context = PrepareContext(opening, config, family?.ActiveWorksetId, names, issues);
        return new LintelCalculation(opening, wall, matchedLintelId, config.RuleConfigName, group, rule,
            config.LintelFamily, type, width, thickness, above, sides, left, right, target, names, context,
            issues);
    }

    /// <inheritdoc />
    public LintelComparison Compare(OpeningLintelMatch pair, LintelCalculation calculation,
        ManagerConfigSnapshot config) {
        var differences = new List<LintelDifference>();
        var opening = pair.Opening;
        var lintel = pair.Lintel;
        void CompareIdentity(string expected, string actual, LintelDifferenceCode mismatch) {
            if(expected is not null && actual is not null && expected != actual) {
                differences.Add(new LintelDifference(mismatch));
            }
        }

        CompareIdentity(calculation?.Family?.UniqueId ?? config.LintelFamily?.UniqueId,
            lintel.Element.FamilyUniqueId, LintelDifferenceCode.Family);
        CompareIdentity(calculation?.Type?.UniqueId, lintel.Element.TypeUniqueId, LintelDifferenceCode.Type);
        CompareMetadata(opening, lintel, calculation, config, differences);
        ComparePosition(pair, differences);
        CompareParameters(lintel, pair.RetainedEnds, calculation, config, differences);
        switch(pair.LockFacts?.IsLocked) {
            case false:
                differences.Add(new LintelDifference(LintelDifferenceCode.LockMissing));
                break;
            case null:
                differences.Add(new LintelDifference(LintelDifferenceCode.LockUnavailable));
                break;
        }
        return new LintelComparison(differences);
    }

    // Сравнивает независимые идентификаторы уровня и стадий. При отсутствии значения сохраняет неизвестность.
    private static void CompareMetadata(ManagerOpeningSnapshot opening, ManagerLintelSnapshot lintel,
        LintelCalculation calculation, ManagerConfigSnapshot config, ICollection<LintelDifference> result) {
        void Check<T>(T expected, T actual, LintelDifferenceCode mismatch, LintelDifferenceCode unavailable) {
            if(expected is null || actual is null) {
                result.Add(new LintelDifference(unavailable));
            } else if(!EqualityComparer<T>.Default.Equals(expected, actual)) {
                result.Add(new LintelDifference(mismatch));
            }
        }

        var target = calculation?.Context;
        Check(target?.LevelId ?? opening.LevelUniqueId, lintel.LevelUniqueId,
            LintelDifferenceCode.Level, LintelDifferenceCode.LevelUnavailable);
        Check(target?.CreatedPhaseId ?? config.Phase?.UniqueId, lintel.CreatedPhaseUniqueId,
            LintelDifferenceCode.CreatedPhase, LintelDifferenceCode.CreatedPhaseUnavailable);
        Check(target?.DemolishedPhaseId ?? opening.Element.Information?.Phases?.DemolishedId,
            lintel.Element.Information?.Phases?.DemolishedId,
            LintelDifferenceCode.DemolishedPhase, LintelDifferenceCode.DemolishedPhaseUnavailable);
    }

    // Использует измерения поиска пары. Инженерный порог действует только при сравнении положения.
    private static void ComparePosition(OpeningLintelMatch pair, ICollection<LintelDifference> result) {
        var contact = pair.Contacts.FirstOrDefault(value => value.OpeningId == pair.Opening.Element.UniqueId
            && value.LintelId == pair.Lintel.Element.UniqueId)?.Deviation;
        if(contact is null || !CalculationNumbers.IsFinite(contact.Distance)
            || !CalculationNumbers.IsFinite(contact.CenterAngle) || !CalculationNumbers.IsFinite(contact.HeightAngle)
            || !CalculationNumbers.IsFinite(contact.LongitudinalAngle)) {
            result.Add(new LintelDifference(LintelDifferenceCode.GeometryUnavailable));
            return;
        }
        double distanceLimit = SystemPluginConfig.CalculationPositionToleranceMm + SystemPluginConfig.MatchRoundoffMm;
        double angleLimit = SystemPluginConfig.MatchAngleToleranceDegrees + SystemPluginConfig.MatchRoundoffDegrees;
        if(Math.Abs(contact.AlongMm) > distanceLimit) {
            result.Add(new LintelDifference(LintelDifferenceCode.Along));
        }
        if(Math.Abs(contact.AcrossMm) > distanceLimit) {
            result.Add(new LintelDifference(LintelDifferenceCode.Across));
        }
        if(Math.Abs(contact.HeightMm) > distanceLimit) {
            result.Add(new LintelDifference(LintelDifferenceCode.Height));
        }
        if(Math.Max(contact.CenterAngle, Math.Max(contact.LongitudinalAngle, contact.HeightAngle)) > angleLimit) {
            result.Add(new LintelDifference(LintelDifferenceCode.Orientation));
        }
    }

    // Сравнивает параметры с расчётом. Опирание сопоставимо только при подтверждённом совпадении ширины.
    private static void CompareParameters(ManagerLintelSnapshot lintel, RetainedLintelEnds retainedEnds,
        LintelCalculation calculation,
        ManagerConfigSnapshot config, ICollection<LintelDifference> result) {
        var values = (calculation?.GetComparableParameters(lintel.Geometry?.Placement) ?? [])
            .ToDictionary(value => value.Key);
        if(calculation?.IsApplicable == true && values.Count != calculation.Parameters.Count) {
            result.Add(new LintelDifference(LintelDifferenceCode.SideParametersUnavailable));
        }
        bool widthMatches = CompareLength(nameof(LintelFamilyConfig.LintelWidth), lintel.OpeningWidthMm,
            ManagerCorrection.Width);
        CompareLength(nameof(LintelFamilyConfig.LintelThickness), lintel.WallThicknessMm, ManagerCorrection.Thickness);
        if(!retainedEnds.HasFlag(RetainedLintelEnds.Left)) {
            if(widthMatches) {
                CompareLength(nameof(LintelFamilyConfig.LintelLeftOffset), lintel.LeftBearingMm,
                    ManagerCorrection.Bearing);
            }
            CompareFlag(nameof(LintelFamilyConfig.LintelLeftCorner), lintel.LeftCorner);
            CompareFlag(nameof(LintelFamilyConfig.LintelLeftWelding), lintel.LeftWelding);
        }
        if(!retainedEnds.HasFlag(RetainedLintelEnds.Right)) {
            if(widthMatches) {
                CompareLength(nameof(LintelFamilyConfig.LintelRightOffset), lintel.RightBearingMm,
                    ManagerCorrection.Bearing);
            }
            CompareFlag(nameof(LintelFamilyConfig.LintelRightCorner), lintel.RightCorner);
            CompareFlag(nameof(LintelFamilyConfig.LintelRightWelding), lintel.RightWelding);
        }

        bool CompareLength(string key, double? actual, ManagerCorrection dimension) {
            if(actual is not double length || !CalculationNumbers.IsFinite(length)) {
                Add(LintelDifferenceCode.ParameterUnavailable, key);
                return false;
            }
            if(!values.TryGetValue(key, out var value) || value.LengthMm is not double expected) {
                return false;
            }
            if(Math.Abs(expected - length) <= SystemPluginConfig.CalculationLengthToleranceMm
                + SystemPluginConfig.MatchRoundoffMm) {
                return true;
            }
            Add(dimension == ManagerCorrection.Width ? LintelDifferenceCode.Width : LintelDifferenceCode.Length,
                key, dimension);
            return false;
        }

        void CompareFlag(string key, int? actual) {
            if(actual is not (0 or 1)) {
                Add(LintelDifferenceCode.ParameterUnavailable, key);
            } else if(values.TryGetValue(key, out var value) && value.YesNo is bool expected
                && expected != (actual == 1)) {
                Add(LintelDifferenceCode.Flag, key);
            }
        }

        void Add(LintelDifferenceCode code, string key, ManagerCorrection dimension = ManagerCorrection.None) {
            config.Parameters.TryGetValue(key, out string name);
            result.Add(new LintelDifference(code, name ?? key, dimension));
        }
    }

    // Проверяет непрерывную кладку именно над верхним центром проёма.
    private static void CheckCentralMasonry(CentralMasonry masonry, double minimum,
        ICollection<CalculationIssue> issues) {
        if(!CalculationNumbers.IsFinite(minimum) || minimum < 0) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InvalidConfiguration,
                detail: nameof(ManagerConfigSnapshot.MinimalHeightAboveOpeningMm)));
        } else if(masonry?.HeightMm is not double height || !CalculationNumbers.IsFinite(height) || height < 0) {
            issues.Add(new CalculationIssue(CalculationIssueCode.GeometryUnavailable,
                detail: masonry?.Limitation ?? nameof(AboveOpeningFacts.Masonry)));
        } else if(height == 0
            || (height < minimum && !(masonry.MinimumMm == minimum && masonry.MeetsMinimum == true))) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InsufficientCentralMasonry));
        }
    }

    // Ограничивает фактическую длину правилом и независимо определяет крепление одной стороны.
    private static BearingResult CalculateBearing(BearingSide side, SideBearingFacts facts, double ruleMm,
        double minimum, ICollection<CalculationIssue> issues) {
        if(facts?.AvailableMm is not double available || facts.AvailableFeet is not double availableFeet
            || facts.Side != side || !CalculationNumbers.IsFinite(available) || available < 0) {
            issues.Add(new CalculationIssue(CalculationIssueCode.GeometryUnavailable, side, facts?.Limitation));
            return null;
        }
        double limited = Math.Min(available, ruleMm);
        double rounded = CalculationNumbers.FloorBearing(limited, availableFeet, ruleMm, facts.RoundoffFeet);
        var contact = facts.RuleMm == ruleMm ? facts.Contact : BearingContact.Unavailable;
        bool? corner = null;
        bool? welding = null;
        if(rounded == ruleMm) {
            corner = welding = false;
        } else if(contact is BearingContact.Concrete or BearingContact.Steel) {
            corner = true;
            welding = contact == BearingContact.Steel;
        } else if(contact is BearingContact.Ambiguous or BearingContact.Unavailable) {
            issues.Add(new CalculationIssue(contact == BearingContact.Ambiguous
                ? CalculationIssueCode.GeometryAmbiguous : CalculationIssueCode.GeometryUnavailable,
                side, facts.Limitation));
        } else {
            corner = welding = false;
        }
        if(corner == false && rounded < minimum) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InsufficientSideMasonry, side));
            issues.Add(new CalculationIssue(CalculationIssueCode.NoEligibleAttachment, side));
            corner = welding = null;
        }
        return new BearingResult(side, available, ruleMm, limited, rounded, minimum, contact, corner, welding);
    }

    // Разрешает имя типа только в настроенном семействе и проверяет его доступность в текущей модели.
    private static ConfigElementKey ResolveType(LintelRuleSnapshot rule, ManagerConfigSnapshot config,
        LintelFamilyFacts family, ICollection<CalculationIssue> issues) {
        var matches = config.LintelTypes.Where(type => type.Name == rule.LintelType).Take(2).ToArray();
        if(matches.Length == 1 && family?.TypeIds.Contains(matches[0].UniqueId) == true) {
            return matches[0];
        }
        issues.Add(new CalculationIssue(matches.Length > 1 || string.IsNullOrWhiteSpace(rule.LintelType)
            ? CalculationIssueCode.InvalidConfiguration : CalculationIssueCode.LintelTypeUnavailable,
            detail: rule.LintelType));
        return null;
    }

    // Использует измеренные плоскости проёма, когда положение начала и стороны семейства подтверждены.
    private static LintelTarget CalculateTarget(GeometryFacts geometry, int? familyLeftSign,
        ICollection<CalculationIssue> issues) {
        var cut = geometry?.Cut;
        var planes = cut?.TargetPlanes;
        if(planes is null || geometry.Wall is null
            || !CalculationNumbers.IsFinite(cut.Width) || !CalculationNumbers.IsFinite(cut.Height)
            || cut.Width <= 0 || cut.Height <= 0 || !IsFinite(planes.Center)
            || !HasWallAxes(planes, geometry.Wall)) {
            issues.Add(new CalculationIssue(CalculationIssueCode.GeometryUnavailable,
                detail: geometry?.Limitation ?? nameof(GeometryFacts.Cut)));
            planes = null;
        }
        if(familyLeftSign is not (-1 or 1)) {
            issues.Add(new CalculationIssue(CalculationIssueCode.FamilySemanticsUnavailable));
            return null;
        }
        return planes is null ? null : new LintelTarget(planes, familyLeftSign.Value);
    }

    // Требует правую горизонтальную систему стены и направленную вверх ось высоты.
    private static bool HasWallAxes(AlignmentPlanes planes, WallFrame wall) {
        double epsilon = SystemPluginConfig.CoordinateRoundoffFactor;
        var along = planes.CenterNormal;
        var normal = planes.LongitudinalNormal;
        var up = planes.HeightNormal;
        return Math.Abs(along.X * along.X + along.Y * along.Y - 1) <= epsilon
            && along.Z == 0 && normal.X == -along.Y && normal.Y == along.X && normal.Z == 0
            && up.X == 0 && up.Y == 0 && up.Z == 1
            && along.X == wall.Along.X && along.Y == wall.Along.Y;
    }

    // Проверяет координаты независимо от API Revit.
    private static bool IsFinite(GeometryPoint point) {
        return CalculationNumbers.IsFinite(point.X) && CalculationNumbers.IsFinite(point.Y)
            && CalculationNumbers.IsFinite(point.Z);
    }

    // Запрещает повторное назначение одного параметра разным ролям и возвращает проверенные имена.
    private static IReadOnlyDictionary<string, string> CheckParameters(ManagerConfigSnapshot config,
        LintelFamilyFacts family, ICollection<CalculationIssue> issues) {
        var names = new Dictionary<string, string>();
        var duplicateNames = LintelParameterSpec.All
            .Where(spec => config.Parameters.ContainsKey(spec.Key)).Select(spec => config.Parameters[spec.Key])
            .GroupBy(name => name, StringComparer.Ordinal).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToHashSet();
        foreach(var spec in LintelParameterSpec.All) {
            config.Parameters.TryGetValue(spec.Key, out string name);
            LintelParameterState state = LintelParameterState.Unavailable;
            if(string.IsNullOrWhiteSpace(name)) {
                state = LintelParameterState.Missing;
            } else if(duplicateNames.Contains(name)) {
                state = LintelParameterState.Ambiguous;
            } else if(family?.Parameters.TryGetValue(spec.Key, out var fact) == true && fact.Name == name) {
                state = fact.State;
            }
            if(state == LintelParameterState.Writable) {
                names.Add(spec.Key, name);
            } else {
                issues.Add(new CalculationIssue(CalculationIssueCode.ParameterUnavailable,
                    detail: string.IsNullOrWhiteSpace(name) ? spec.Key : name, parameterState: state));
            }
        }
        return names;
    }

    // Фиксирует уровень и снос проёма, стадию из настроек, активный набор и значение фиксации.
    private static LintelPlacementContext PrepareContext(ManagerOpeningSnapshot opening, ManagerConfigSnapshot config,
        long? worksetId, IReadOnlyDictionary<string, string> names, ICollection<CalculationIssue> issues) {
        const string key = nameof(LintelManagerConfig.LintelFixParam);
        if(string.IsNullOrEmpty(opening.LevelUniqueId) || string.IsNullOrEmpty(config.Phase?.UniqueId)
            || opening.Element.Information?.Phases?.DemolishedId is not long demolished
            || worksetId is not long workset) {
            issues.Add(new CalculationIssue(CalculationIssueCode.InvalidPlacementContext));
            return null;
        }
        var fixation = names.TryGetValue(key, out string name)
            ? new LintelParameterValue(key, name, LintelParameterKind.YesNo, null, false) : null;
        return new LintelPlacementContext(opening.LevelUniqueId, config.Phase.UniqueId, demolished, workset, fixation);
    }
}
