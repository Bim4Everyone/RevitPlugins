using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Причины, блокирующие применение расчёта.</summary>
internal enum CalculationIssueCode {
    WallTypeUnresolved, NoPlacementGroup, ConflictingPlacementGroups, NoWidthRule, ConflictingRules,
    InvalidConfiguration, InvalidDimensions, LintelTypeUnavailable, GeometryUnavailable, GeometryAmbiguous,
    InsufficientCentralMasonry, InsufficientSideMasonry, NoEligibleAttachment, FamilySemanticsUnavailable,
    ParameterUnavailable, InvalidPlacementContext
}

internal sealed class CalculationIssue(CalculationIssueCode code, BearingSide? side = null, string detail = null,
    LintelParameterState? parameterState = null) {
    public CalculationIssueCode Code { get; } = code;
    public BearingSide? Side { get; } = side;
    public string Detail { get; } = detail;
    public LintelParameterState? ParameterState { get; } = parameterState;
}

/// <summary>Исходные и итоговые длины в мм, контакты и крепления одной физической стороны.</summary>
internal sealed class BearingResult(BearingSide side, double availableMm, double ruleMm, double limitedMm,
    double bearingMm, double minimumMm, BearingContact contact, bool? corner, bool? welding) {
    public BearingSide Side { get; } = side;
    public double AvailableMm { get; } = availableMm;
    public double RuleMm { get; } = ruleMm;
    public double LimitedMm { get; } = limitedMm;
    public double BearingMm { get; } = bearingMm;
    public double MinimumMm { get; } = minimumMm;
    public BearingContact Contact { get; } = contact;
    public bool? Corner { get; } = corner;
    public bool? Welding { get; } = welding;
}

/// <summary>Геометрическая привязка и направление стороны, обозначенной левой в семействе.</summary>
internal sealed class LintelTarget(AlignmentPlanes planes, int familyLeftSign) {
    public AlignmentPlanes Planes { get; } = planes;
    public int FamilyLeftSign { get; } = familyLeftSign;
    public BearingSide FamilyLeftAtTarget => FamilyLeftSign < 0 ? BearingSide.Left : BearingSide.Right;

    /// <summary>Разрешает физическую сторону для левого параметра при известном преобразовании экземпляра.</summary>
    public BearingSide? GetPhysicalLeft(InstancePlacement placement) {
        if(placement?.TransformAxes is not { } axes) {
            return null;
        }
        double x = Dot(axes.CenterNormal, Planes.CenterNormal);
        double z = Dot(axes.HeightNormal, Planes.HeightNormal);
        double limit = Math.Cos(SystemPluginConfig.MatchAngleToleranceDegrees * Math.PI / 180)
            - SystemPluginConfig.CoordinateRoundoffFactor;
        if(!IsParallel(Math.Abs(x), limit) || !IsParallel(z, limit)) {
            return null;
        }
        // BasisX включает результат отражения и flips. Повторное применение флагов поменяло бы стороны дважды.
        return x * FamilyLeftSign < 0 ? BearingSide.Left : BearingSide.Right;
    }

    // Проверяет сопоставимость направлений с учётом машинной погрешности скалярного произведения.
    private static bool IsParallel(double dot, double limit) {
        return dot >= limit && dot <= 1 + SystemPluginConfig.CoordinateRoundoffFactor;
    }

    // Возвращает косинус угла между единичными направлениями.
    private static double Dot(GeometryPoint a, GeometryPoint b) {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}

/// <summary>Подготовленное значение сопоставленного параметра экземпляра.</summary>
internal sealed class LintelParameterValue(string key, string name, LintelParameterKind kind, double? lengthMm,
    bool? yesNo) {
    public string Key { get; } = key;
    public string Name { get; } = name;
    public LintelParameterKind Kind { get; } = kind;
    public double? LengthMm { get; } = lengthMm;
    public bool? YesNo { get; } = yesNo;
}

/// <summary>Размещение по идентификаторам исходного документа. Стадия сноса может быть не назначена.</summary>
internal sealed class LintelPlacementContext(string levelId, string createdPhaseId, long demolishedPhaseId,
    long activeWorksetId, LintelParameterValue fixation) {
    public string LevelId { get; } = levelId;
    public string CreatedPhaseId { get; } = createdPhaseId;
    public long DemolishedPhaseId { get; } = demolishedPhaseId;
    public long ActiveWorksetId { get; } = activeWorksetId;
    public LintelParameterValue Fixation { get; } = fixation;
}

/// <summary>Единый эталон параметров и положения для будущих Install, Replace и Compare.</summary>
internal sealed class LintelCalculation(ManagerOpeningSnapshot opening, ManagerWallSnapshot wall,
    string matchedLintelId, string ruleConfigName, PlacementRuleSnapshot group, LintelRuleSnapshot rule,
    ConfigElementKey family, ConfigElementKey type, double? widthMm, double? thicknessMm,
    AboveOpeningFacts above, OpeningBearingFacts sideFacts, BearingResult left, BearingResult right,
    LintelTarget target, IReadOnlyDictionary<string, string> parameterNames, LintelPlacementContext context,
    IEnumerable<CalculationIssue> issues) {
    /// <summary>Сохраняет причину отсутствия перемычки, когда подбор для пустого проёма не нужен.</summary>
    public LintelCalculation(ManagerOpeningSnapshot opening, AboveOpeningFacts above)
        : this(opening, wall: null, matchedLintelId: null, ruleConfigName: null, group: null, rule: null,
            family: null, type: null, widthMm: opening.WidthMm, thicknessMm: null, above: above, sideFacts: null,
            left: null, right: null, target: null, parameterNames: new Dictionary<string, string>(), context: null,
            issues: []) {
        LintelNotRequired = true;
    }

    public ManagerOpeningSnapshot Opening { get; } = opening;
    public ManagerWallSnapshot Wall { get; } = wall;
    public string MatchedLintelId { get; } = matchedLintelId;
    public string RuleConfigName { get; } = ruleConfigName;
    public PlacementRuleSnapshot Group { get; } = group;
    public LintelRuleSnapshot Rule { get; } = rule;
    public ConfigElementKey Family { get; } = family;
    public ConfigElementKey Type { get; } = type;
    public double? WidthMm { get; } = widthMm;
    public double? ThicknessMm { get; } = thicknessMm;
    public AboveOpeningFacts Above { get; } = above;
    public OpeningBearingFacts SideFacts { get; } = sideFacts;
    public BearingResult Left { get; } = left;
    public BearingResult Right { get; } = right;
    public LintelTarget Target { get; } = target;
    public IReadOnlyList<LintelParameterValue> Parameters { get; } = PrepareParameters(parameterNames, widthMm,
        thicknessMm, left, right, target?.FamilyLeftAtTarget);
    public LintelPlacementContext Context { get; } = context;
    public IReadOnlyList<CalculationIssue> Issues { get; } = issues.ToList().AsReadOnly();
    public bool LintelNotRequired { get; } = above?.LimitingStructure is not null;
    public bool IsApplicable => !LintelNotRequired && Issues.Count == 0;

    /// <summary>Проецирует тот же расчёт на стороны подтверждённого положения этого семейства.</summary>
    /// <param name="placement">Положение экземпляра выбранного семейства.</param>
    /// <returns>Доступные значения в системе экземпляра либо пустой список при неопределённых осях.</returns>
    public IReadOnlyList<LintelParameterValue> GetParametersFor(InstancePlacement placement) {
        var physical = Target?.GetPhysicalLeft(placement);
        return IsApplicable && physical.HasValue ? GetParametersForSide(physical) : [];
    }

    /// <summary>Возвращает подтверждённые эталонные значения для ориентации существующей перемычки.</summary>
    public IReadOnlyList<LintelParameterValue> GetComparableParameters(InstancePlacement placement) {
        return GetParametersForSide(Target?.GetPhysicalLeft(placement));
    }

    // Использует измеренные стороны для фактической ориентации или уже подготовленные значения установки.
    private IReadOnlyList<LintelParameterValue> GetParametersForSide(BearingSide? physicalLeft) {
        return physicalLeft == Target?.FamilyLeftAtTarget ? Parameters
            : PrepareParameters(parameterNames, WidthMm, ThicknessMm, Left, Right, physicalLeft);
    }

    // Каждой роли семейства назначает значение с того конца, в который она попадает в модели.
    private static IReadOnlyList<LintelParameterValue> PrepareParameters(IReadOnlyDictionary<string, string> names,
        double? width, double? thickness, BearingResult left, BearingResult right, BearingSide? physicalLeft) {
        var familyLeft = physicalLeft == BearingSide.Left ? left : physicalLeft == BearingSide.Right ? right : null;
        var familyRight = physicalLeft == BearingSide.Left ? right : physicalLeft == BearingSide.Right ? left : null;
        var result = new List<LintelParameterValue>();
        foreach(var spec in LintelParameterSpec.All) {
            if(!names.TryGetValue(spec.Key, out string name)) {
                continue;
            }
            var side = spec.FamilySide == BearingSide.Left ? familyLeft : familyRight;
            var (length, flag) = spec.Key switch {
                nameof(LintelFamilyConfig.LintelWidth) => (width, (bool?) null),
                nameof(LintelFamilyConfig.LintelThickness) => (thickness, null),
                nameof(LintelFamilyConfig.LintelLeftOffset) or nameof(LintelFamilyConfig.LintelRightOffset) =>
                    (side?.BearingMm, null),
                nameof(LintelFamilyConfig.LintelLeftCorner) or nameof(LintelFamilyConfig.LintelRightCorner) =>
                    (null, side?.Corner),
                nameof(LintelFamilyConfig.LintelLeftWelding) or nameof(LintelFamilyConfig.LintelRightWelding) =>
                    (null, side?.Welding),
                _ => ((double?) null, null)
            };
            if(length.HasValue || flag.HasValue) {
                result.Add(new LintelParameterValue(spec.Key, name, spec.Kind, length, flag));
            }
        }
        return result.AsReadOnly();
    }
}
