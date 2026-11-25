using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Независимые результаты сравнения с эталоном размещения.</summary>
internal enum LintelDifferenceCode {
    Family, Type, Width, Length, Flag, ParameterUnavailable, SideParametersUnavailable,
    Level, LevelUnavailable, CreatedPhase, CreatedPhaseUnavailable, DemolishedPhase, DemolishedPhaseUnavailable,
    Along, Across, Height, Orientation, GeometryUnavailable, LockMissing, LockUnavailable
}

internal sealed class LintelDifference(LintelDifferenceCode code, string parameter = null,
    ManagerCorrection dimension = ManagerCorrection.None) {
    public LintelDifferenceCode Code { get; } = code;
    public string Parameter { get; } = parameter;
    public ManagerCorrection Correction { get; } = code switch {
        LintelDifferenceCode.Width => ManagerCorrection.Width,
        LintelDifferenceCode.Length => dimension,
        LintelDifferenceCode.Flag => ManagerCorrection.Attachments,
        LintelDifferenceCode.Along or LintelDifferenceCode.Across or LintelDifferenceCode.Height
            or LintelDifferenceCode.Orientation => ManagerCorrection.Position,
        _ => ManagerCorrection.None
    };
}

/// <summary>Сохраняет каждое обнаруженное отличие и отдельно пригодность геометрии.</summary>
internal sealed class LintelComparison(IEnumerable<LintelDifference> differences) {
    public IReadOnlyList<LintelDifference> Differences { get; } = differences.ToList().AsReadOnly();
    public ManagerCorrection Corrections => Differences.Aggregate(ManagerCorrection.None,
        (corrections, difference) => corrections | difference.Correction);
    public bool RequiresReplacement => Differences.Any(item => item.Code is LintelDifferenceCode.Family
        or LintelDifferenceCode.Type
        or LintelDifferenceCode.Level or LintelDifferenceCode.CreatedPhase or LintelDifferenceCode.DemolishedPhase);
    public bool PositionKnown => Differences.All(item => item.Code != LintelDifferenceCode.GeometryUnavailable);
    public bool RequiresPositionCorrection => (Corrections & ManagerCorrection.Position) != ManagerCorrection.None;
    public bool RequiresCorrection => Corrections != ManagerCorrection.None;
    public bool LockMissing => Differences.Any(item => item.Code == LintelDifferenceCode.LockMissing);
}
