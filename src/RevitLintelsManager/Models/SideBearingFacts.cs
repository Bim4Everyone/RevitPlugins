using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Физическая сторона проёма относительно направленной оси стены.</summary>
internal enum BearingSide { Left, Right }

/// <summary>Результат проверки непосредственного примыкания на данной стороне.</summary>
internal enum BearingContact { None, Concrete, Steel, Ambiguous, Unavailable }

/// <summary>Фактически рассмотренное тело. Дистанции в футах от кромки проёма наружу.</summary>
internal sealed class BearingIntersection(string uniqueId, string typeId, string category, bool concrete, bool steel,
    IReadOnlyList<MaterialInterval> intervals) {
    public string UniqueId { get; } = uniqueId;
    public string TypeUniqueId { get; } = typeId;
    public string Category { get; } = category;
    public bool Concrete { get; } = concrete;
    public bool Steel { get; } = steel;
    public IReadOnlyList<MaterialInterval> Intervals { get; } = intervals;
}

/// <summary>Независимые числовые факты одной стороны; null сохраняет неизвестное значение.</summary>
internal sealed class SideBearingFacts {
    /// <summary>Оценивает измеренную кладку и материал непосредственно за её концом.</summary>
    public SideBearingFacts(BearingSide side, GeometryPoint? start, GeometryPoint? direction,
        ContinuousMaterial masonry, double? ruleMm, double roundoffFeet,
        IReadOnlyList<BearingIntersection> intersections, bool incomplete) {
        Side = side;
        Start = start;
        Direction = direction;
        WallIntervals = masonry.Intervals;
        AvailableFeet = masonry.LengthFeet;
        AvailableMm = masonry.LengthMm;
        RoundoffFeet = roundoffFeet;
        MinimumMm = masonry.MinimumMm;
        RuleMm = ruleMm;
        Intersections = intersections.ToList().AsReadOnly();
        if(!AvailableMm.HasValue || !ruleMm.HasValue) {
            Limitation = masonry.Limitation ?? "Не определён диапазон правила для ширины проёма и типа стены.";
            Contact = BearingContact.Unavailable;
            return;
        }
        BearingMm = Math.Min(AvailableMm.Value, ruleMm.Value);
        double roundoffMm = roundoffFeet * SystemPluginConfig.MillimetersPerFoot;
        MeetsMinimum = BearingMm.Value + roundoffMm >= MinimumMm;
        if(AvailableMm.Value + roundoffMm < ruleMm.Value) {
            if(incomplete || intersections.Any(hit => hit.Intervals is null)) {
                Contact = BearingContact.Unavailable;
                Limitation = "Не удалось проверить геометрию всех возможных примыканий.";
                return;
            }
            double end = AvailableFeet.Value;
            // Стык тел сопоставляется в пределах точности пересечения. Тело должно продолжаться за кладкой.
            var contacts = intersections.Where(hit => hit.Intervals.Any(interval =>
                interval.Start <= end + SystemPluginConfig.GeometryLengthEpsilonFeet
                && interval.End > end + roundoffFeet)).ToList();
            bool concrete = contacts.Any(hit => hit.Concrete);
            bool steel = contacts.Any(hit => hit.Steel);
            if(concrete && steel) {
                Contact = BearingContact.Ambiguous;
                Limitation = "На одной границе подтверждены и ЖБ, и металлическое примыкания.";
                return;
            }
            Contact = concrete ? BearingContact.Concrete : steel ? BearingContact.Steel : BearingContact.None;
            StructureUniqueId = contacts.Count == 1 ? contacts[0].UniqueId : null;
        }
        Corner = Contact is BearingContact.Concrete or BearingContact.Steel;
        Welding = Contact == BearingContact.Steel;
        IsSupported = MeetsMinimum.Value || Corner.Value;
    }

    public BearingSide Side { get; }
    public GeometryPoint? Start { get; }
    public GeometryPoint? Direction { get; }
    public double MinimumMm { get; }
    public double? AvailableMm { get; }
    public double? AvailableFeet { get; }
    public double RoundoffFeet { get; }
    public double? RuleMm { get; }
    public double? BearingMm { get; }
    public bool? MeetsMinimum { get; }
    public bool? IsSupported { get; }
    public BearingContact Contact { get; }
    public string StructureUniqueId { get; }
    public bool? Corner { get; }
    public bool? Welding { get; }
    public string Limitation { get; }
    public IReadOnlyList<MaterialInterval> WallIntervals { get; }
    public IReadOnlyList<BearingIntersection> Intersections { get; }
}

/// <summary>Физические стороны одного проёма до сопоставления с параметрами семейства.</summary>
internal sealed class OpeningBearingFacts(string openingId, SideBearingFacts left, SideBearingFacts right) {
    public string OpeningId { get; } = openingId;
    public SideBearingFacts Left { get; } = left;
    public SideBearingFacts Right { get; } = right;
}

internal sealed partial class ManagerProjectSnapshot {
    /// <summary>Публикует факты обеих сторон в том же поколении, что и верхние конструкции.</summary>
    public ManagerProjectSnapshot(IEnumerable<ManagerOpeningSnapshot> openings,
        IEnumerable<ManagerLintelSnapshot> lintels, IEnumerable<ManagerWallSnapshot> walls,
        IEnumerable<ManagerStructureSnapshot> structures, int unreadableCount, IEnumerable<OpeningLintelMatch> matches,
        IEnumerable<AboveOpeningFacts> above, IEnumerable<OpeningBearingFacts> sides)
        : this(openings, lintels, walls, structures, unreadableCount, matches, above) {
        SideBearings = new ReadOnlyDictionary<string, OpeningBearingFacts>(sides.ToDictionary(f => f.OpeningId));
    }

    public IReadOnlyDictionary<string, OpeningBearingFacts> SideBearings { get; } =
        new ReadOnlyDictionary<string, OpeningBearingFacts>(new Dictionary<string, OpeningBearingFacts>());
}
