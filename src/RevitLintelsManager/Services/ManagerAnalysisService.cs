using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Назначает предупреждения и действия менеджера по независимым снимкам.</summary>
internal sealed class ManagerAnalysisService(ILintelCalculationService calculationService) : IManagerAnalysisService {
    /// <inheritdoc />
    public ManagerAnalyzedRow Analyze(Guid documentToken, OpeningLintelMatch match, LintelCalculation calculation,
        ManagerConfigSnapshot config) => AnalyzeForProcessing(documentToken, match, calculation, config, out _);

    /// <inheritdoc />
    public ManagerAnalyzedRow AnalyzeForProcessing(Guid documentToken, OpeningLintelMatch match, LintelCalculation calculation,
        ManagerConfigSnapshot config, out LintelComparison comparison) {
        var opening = match.Opening;
        var lintel = match.Lintel;
        var openingInfo = opening?.Element.Information;
        var lintelInfo = lintel?.Element.Information;
        bool isFixed = openingInfo?.IsFixed == true || lintelInfo?.IsFixed == true;
        var openingDiagnostics = new List<ManagerDiagnostic>();
        var lintelDiagnostics = new List<ManagerDiagnostic>();
        var rowDiagnostics = new List<ManagerDiagnostic>();
        var issues = GetVisibleIssues(match, calculation);
        bool blocked = AddRestrictions(opening?.Element, lintel?.Element, openingDiagnostics, lintelDiagnostics);
        AddCalculationDiagnostics(opening, calculation is not null, issues, openingDiagnostics);
        bool lintelNotRequired = calculation?.LintelNotRequired == true && lintel is null
            && (match.State == MatchState.NoCandidate || (match.State == MatchState.Incomplete
                && match.Contacts.Count == 0 && calculation.Above.LimitingStructure is not null));
        if(lintelNotRequired) {
            rowDiagnostics.Add(GetOmissionReason(calculation.Above, config.MinimalHeightAboveOpeningMm));
        } else {
            bool hasOpeningContext = lintel is null && AddOpeningContext(calculation, rowDiagnostics);
            AddMatchDiagnostic(match, hasOpeningContext, rowDiagnostics);
        }
        bool isPair = IsPair(match);
        comparison = isPair ? calculationService.Compare(match, calculation, config) : null;
        if(comparison is not null) {
            lintelDiagnostics.AddRange(comparison.Differences
                .Select(difference => Diagnostic($"Compare.{difference.Code}", difference.Parameter)));
        }

        var allowed = new List<ManagerAction>();
        var defaultAction = AddLintelActions(match, calculation, comparison, issues.Count == 0, blocked, allowed);
        allowed.Add(isFixed ? ManagerAction.ClearFixation : ManagerAction.SetFixation);
        allowed.Add(ManagerAction.Skip);
        bool unresolvedMatch = !lintelNotRequired && !isPair && match.State != MatchState.NoCandidate;
        var openingStatus = opening is null && match.Conflict != OpeningConflict.None
            ? ManagerElementStatus.MultipleOpenings
            : ElementStatus(opening?.Element, openingDiagnostics, unresolvedMatch, isFixed);
        var lintelStatus = lintelNotRequired ? ManagerElementStatus.LintelNotRequired
            : lintel is null && match.Conflict != OpeningConflict.None ? ManagerElementStatus.OpeningConflict
            : ElementStatus(lintel?.Element, lintelDiagnostics, unresolvedMatch, isFixed, lintelInfo?.IsInGroup);
        var left = opening is null ? null : new ManagerElementPresentation(opening.Element.TypeName, openingStatus,
            openingInfo, openingDiagnostics, opening.WidthMm, opening.HeightMm);
        var right = lintel is null ? null : new ManagerElementPresentation(lintel.Element.TypeName, lintelStatus,
            lintelInfo, lintelDiagnostics, lintel.OpeningWidthMm, thicknessMm: lintel.WallThicknessMm);
        var key = new ManagerItemKey(documentToken, opening?.Element.UniqueId, lintel?.Element.UniqueId);
        var groupKey = new ManagerGroupKey(opening?.Element.TypeUniqueId, lintel?.Element.TypeUniqueId,
            openingStatus, lintelStatus, lintelInfo?.IsFixed);
        var snapshot = new ManagerItemSnapshot(key, groupKey, rowDiagnostics, allowed, defaultAction,
            comparison?.Corrections ?? ManagerCorrection.None);
        return new ManagerAnalyzedRow(snapshot, left, right) { Match = match, Comparison = comparison };
    }

    // Учитывает только взаимно однозначное сопоставление из анализа всего исходного документа.
    private static bool IsPair(OpeningLintelMatch match) {
        return match.Opening is not null && match.Lintel is not null
            && match.State is MatchState.Confirmed or MatchState.Shifted;
    }

    // Фиксация, недоступность признака и группа модели ограничивают операции с перемычкой.
    private static bool AddRestrictions(ManagerFamilySnapshot opening, ManagerFamilySnapshot lintel,
        ICollection<ManagerDiagnostic> openingDiagnostics, ICollection<ManagerDiagnostic> lintelDiagnostics) {
        bool prohibited = AddFixationDiagnostic(opening, openingDiagnostics);
        prohibited |= AddFixationDiagnostic(lintel, lintelDiagnostics);
        if(lintel is not null && lintel.Information?.IsInGroup != false) {
            lintelDiagnostics.Add(Diagnostic(lintel.Information?.IsInGroup == true ? "Grouped" : "GroupUnavailable"));
            prohibited = true;
        }
        return prohibited;
    }

    // Сохраняет отдельное предупреждение о фиксации на каждой стороне.
    private static bool AddFixationDiagnostic(ManagerFamilySnapshot element,
        ICollection<ManagerDiagnostic> diagnostics) {
        if(element is null || element.Information?.IsFixed == false) {
            return false;
        }
        diagnostics.Add(Diagnostic(element.Information?.IsFixed == true ? "Fixed" : "FixUnavailable"));
        return true;
    }

    // Убирает из проверки существующей пары причины подбора на конце, который останется на месте.
    private static IReadOnlyCollection<CalculationIssue> GetVisibleIssues(OpeningLintelMatch match,
        LintelCalculation calculation) {
        if(calculation is null) {
            return [];
        }
        if(match.RetainedEnds == RetainedLintelEnds.None
            || calculation.Target?.GetPhysicalLeft(match.Lintel?.Geometry?.Placement) is not { } physicalLeft) {
            return calculation.Issues;
        }
        return calculation.Issues.Where(issue => !issue.Side.HasValue || !match.RetainedEnds.HasFlag(
            issue.Side == physicalLeft ? RetainedLintelEnds.Left : RetainedLintelEnds.Right)).ToList();
    }

    // Показывает причины подбора для проверяемых сторон.
    private static void AddCalculationDiagnostics(ManagerOpeningSnapshot opening, bool calculationAvailable,
        IReadOnlyCollection<CalculationIssue> issues,
        ICollection<ManagerDiagnostic> diagnostics) {
        if(opening is null) {
            return;
        }
        if(!calculationAvailable) {
            diagnostics.Add(Diagnostic("CalculationUnavailable"));
            return;
        }
        foreach(var issue in issues) {
            if(issue.Side is BearingSide side && issue.Code == CalculationIssueCode.GeometryAmbiguous) {
                diagnostics.Add(Diagnostic($"Calculation.ContactAmbiguous.{side}"));
            } else if(issue.Side is BearingSide bearingSide && issue.Code == CalculationIssueCode.GeometryUnavailable) {
                diagnostics.Add(Diagnostic($"Calculation.SideGeometryUnavailable.{bearingSide}"));
            } else {
                string suffix = issue.Side.HasValue ? $".{issue.Side}" : string.Empty;
                diagnostics.Add(Diagnostic($"Calculation.{issue.Code}{suffix}",
                    issue.Code == CalculationIssueCode.ParameterUnavailable ? issue.Detail : null));
            }
        }
    }

    // Объясняет отсутствие перемычки по ближайшей конструкции или по измеренной высоте кладки.
    private static ManagerDiagnostic GetOmissionReason(AboveOpeningFacts above, double minimumMm) {
        string code;
        string[] arguments = [];
        if(above.LimitingStructure is { } nearest) {
            code = nearest.IsSlab ? (nearest.IsLinked ? "LinkedSlabAbove" : "SlabAbove") : $"Upper.{nearest.Category}";
        } else if(above.Masonry.HasMaterial == false) {
            code = "NoMasonryAbove";
        } else {
            code = "MasonryBelowInstallationThreshold";
            arguments = [above.Masonry.HeightMm.Value.ToString("0.##", CultureInfo.CurrentCulture),
                minimumMm.ToString("0.##", CultureInfo.CurrentCulture)];
        }
        return new ManagerDiagnostic($"Manager.Diagnostic.{code}", arguments, isInformational: true);
    }

    // Показывает подтверждённые причины в предупреждении пустого поля перемычки.
    private static bool AddOpeningContext(LintelCalculation calculation, ICollection<ManagerDiagnostic> diagnostics) {
        if(calculation is null) {
            return false;
        }
        int previousCount = diagnostics.Count;
        if(calculation.Issues.Any(issue => issue.Code == CalculationIssueCode.InsufficientCentralMasonry)) {
            var above = calculation.Above;
            if(above?.Masonry?.Intervals?.Count == 0) {
                diagnostics.Add(Diagnostic(above.Upper?.State == UpperStructureState.Absent
                    ? "NoMasonryOrUpperStructure" : "NoMasonryAbove"));
            } else {
                diagnostics.Add(Diagnostic("Calculation.InsufficientCentralMasonry"));
            }
            var nearest = above?.Upper?.Nearest;
            if(nearest?.Count == 1 && nearest[0].IsSlab) {
                diagnostics.Add(Diagnostic(nearest[0].IsLinked ? "LinkedSlabAbove" : "SlabAbove"));
            } else if(above?.Upper?.State == UpperStructureState.Unavailable) {
                diagnostics.Add(Diagnostic("UpperStructureUnavailable"));
            }
        }
        foreach(var bearing in new[] { calculation.Left, calculation.Right }) {
            if(bearing is { Contact: BearingContact.None, AvailableMm: 0 }) {
                diagnostics.Add(Diagnostic($"NoBearing.{bearing.Side}"));
            } else if(bearing is { Corner: false } && bearing.BearingMm < bearing.MinimumMm) {
                diagnostics.Add(Diagnostic($"Calculation.InsufficientSideMasonry.{bearing.Side}"));
            }
        }
        return diagnostics.Count > previousCount;
    }

    // Отличает отсутствие второго элемента от поиска с неподтверждённым вырезом или кандидатами.
    private static void AddMatchDiagnostic(OpeningLintelMatch match, bool hasOpeningContext,
        ICollection<ManagerDiagnostic> diagnostics) {
        string conflictCode = match.Conflict switch {
            OpeningConflict.IntersectingCuts => "OpeningCutConflict",
            OpeningConflict.CompetingOpenings => "CompetingOpenings",
            _ => null
        };
        if(conflictCode is not null) {
            diagnostics.Add(Diagnostic(conflictCode));
            return;
        }
        if(IsPair(match)) {
            return;
        }
        var geometry = match.Opening?.Geometry;
        bool openCut = geometry is { Cut: null } && match.Limitation == geometry.Limitation;
        if(hasOpeningContext && (match.State == MatchState.NoCandidate || (openCut && geometry.OpenTop.HasValue))) {
            return;
        }
        if(match.State == MatchState.NoCandidate && match.Lintel is null) {
            diagnostics.Add(new ManagerDiagnostic("Manager.NoLintel", []));
            return;
        }
        string code = match.State switch {
            MatchState.NoCandidate => "OpeningMissing",
            MatchState.Ambiguous => "MatchAmbiguous",
            MatchState.OneWay => "MatchOneWay",
            MatchState.Incomplete when openCut => "OpeningCutUnavailable",
            _ => "MatchIncomplete"
        };
        diagnostics.Add(Diagnostic(code));
    }

    // Добавляет допустимые операции над перемычкой и выбирает действие по умолчанию.
    private static ManagerAction AddLintelActions(
        OpeningLintelMatch match, LintelCalculation calculation, LintelComparison comparison,
        bool noVisibleIssues, bool prohibited, List<ManagerAction> allowed) {
        if(prohibited || (IsPair(match) && match.Opening.Geometry?.Cut is null)) {
            return ManagerAction.Skip;
        }
        if(match.State == MatchState.NoCandidate && match.Opening is null && match.Lintel is not null) {
            allowed.Add(ManagerAction.Delete);
            return ManagerAction.Delete;
        }
        bool canLock = comparison is { PositionKnown: true, RequiresPositionCorrection: false, LockMissing: true };
        bool canInstall = calculation?.IsApplicable == true;
        bool canCorrectExisting = calculation is { LintelNotRequired: false } && comparison is not null
            && noVisibleIssues;
        if(!canInstall && !canCorrectExisting) {
            if(canLock) {
                allowed.Add(ManagerAction.Lock);
            }
            return ManagerAction.Skip;
        }
        if(match.State == MatchState.NoCandidate && match.Opening is not null && match.Lintel is null) {
            allowed.Add(ManagerAction.Install);
            return ManagerAction.Install;
        }
        if(comparison?.PositionKnown != true) {
            return ManagerAction.Skip;
        }

        if(canInstall) {
            allowed.Add(ManagerAction.Replace);
        }
        if(comparison.RequiresCorrection) {
            allowed.Add(ManagerAction.Correct);
        }
        if(canLock) {
            allowed.Add(ManagerAction.Lock);
        }
        allowed.Add(ManagerAction.Delete);
        return comparison.RequiresReplacement ? (canInstall ? ManagerAction.Replace : ManagerAction.Skip)
            : comparison.RequiresCorrection ? ManagerAction.Correct
            : canLock ? ManagerAction.Lock : ManagerAction.Skip;
    }

    // Основной статус учитывает блокировки, сохраняя все дополнительные предупреждения.
    private static ManagerElementStatus ElementStatus(ManagerFamilySnapshot element,
        IReadOnlyCollection<ManagerDiagnostic> diagnostics, bool unresolved, bool isFixed, bool? grouped = null) {
        if(element is null) {
            return unresolved ? ManagerElementStatus.Warning : ManagerElementStatus.Missing;
        }
        if(isFixed) {
            return ManagerElementStatus.Fixed;
        }
        if(grouped == true) {
            return ManagerElementStatus.Grouped;
        }
        return diagnostics.Count > 0 || unresolved ? ManagerElementStatus.Warning : ManagerElementStatus.Correct;
    }

    // Материализует код и аргумент для существующего механизма локализации.
    private static ManagerDiagnostic Diagnostic(string code, string parameter = null) {
        return new ManagerDiagnostic($"Manager.Diagnostic.{code}", parameter is null ? [] : [parameter]);
    }
}
