using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Состояние измерения верхней конструкции; одинаковая высота сохраняет всех ближайших участников.</summary>
internal enum UpperStructureState { Absent, Found, Ambiguous, Unavailable }

/// <summary>Факты одного проёма. Координаты O и интервалы от него заданы во внутренних единицах Revit.</summary>
internal sealed class AboveOpeningFacts(string openingId, GeometryPoint? origin,
    CentralMasonry masonry, UpperStructureResult upper) {
    /// <summary>Фиксирует прямое примыкание плиты к верху проёма, достаточное для завершения подбора.</summary>
    internal AboveOpeningFacts(string openingId, GeometryPoint origin, UpperStructureResult touchingSlabs)
        : this(openingId, origin, null, touchingSlabs) {
        SlabAtHead = touchingSlabs.Nearest[0];
    }

    internal UpperStructureHit SlabAtHead { get; }
    public string OpeningId { get; } = openingId;
    public GeometryPoint? Origin { get; } = origin;
    public CentralMasonry Masonry { get; } = masonry;
    public UpperStructureResult Upper { get; } = upper;

    /// <summary>Ближайшая конструкция, до которой от верха проёма подтверждено отсутствие кладки.</summary>
    public UpperStructureHit LimitingStructure {
        get {
            if(SlabAtHead is not null) {
                return SlabAtHead;
            }
            if(Masonry?.HasMaterial != false || Upper?.State != UpperStructureState.Found) {
                return null;
            }
            var nearest = Upper.Nearest[0];
            return Masonry.Intervals.Count == 0 || Masonry.Intervals[0].Start >= nearest.DistanceFeet ? nearest : null;
        }
    }

    /// <summary>Определяет, позволяет ли измеренная геометрия пропустить подбор перемычки.</summary>
    public bool CanSkipLintelDesign(bool hasExistingLintel, double minimumMm) {
        return SlabAtHead is not null || (hasExistingLintel ? LimitingStructure is not null
            : Masonry?.IsAtOrBelowMinimum(minimumMm) == true);
    }
}

/// <summary>Материал стены-хоста вдоль центральной вертикали. Неизвестные значения сохраняются как null.</summary>
internal sealed class CentralMasonry(IReadOnlyList<MaterialInterval> intervals, double minimumMm, double roundoffFeet,
    string limitation = null) : ContinuousMaterial(intervals, minimumMm, roundoffFeet, limitation) {
    private readonly double _roundoffMm = Math.Max(SystemPluginConfig.MatchRoundoffMm,
        roundoffFeet * SystemPluginConfig.MillimetersPerFoot);
    public double? HeightMm => LengthMm;

    /// <summary>Проверяет, что измеренная высота кладки не превышает заданного порога установки.</summary>
    public bool IsAtOrBelowMinimum(double minimum) {
        return CalculationNumbers.IsFinite(minimum) && minimum >= 0 && HeightMm is double height
            && height >= 0 && height <= minimum + _roundoffMm;
    }
}

/// <summary>Первый вход и все проверенные интервалы внутри конкретной верхней конструкции.</summary>
internal sealed class UpperStructureHit(string elementId, string typeId, string category,
    IReadOnlyList<MaterialInterval> intervals, bool isSlab = false, bool isLinked = false) {
    public string UniqueId { get; } = elementId;
    public string TypeUniqueId { get; } = typeId;
    public string Category { get; } = category;
    public IReadOnlyList<MaterialInterval> Intervals { get; } = intervals;
    public bool IsSlab { get; } = isSlab;
    public bool IsLinked { get; } = isLinked;
    public double DistanceFeet => Intervals[0].Start;
}

/// <summary>Ближайшие подтверждённые пересечения и степень достоверности их положения.</summary>
internal sealed class UpperStructureResult {
    /// <summary>Сравнивает входы и нижнюю границу возможного, но неподтверждённого контакта.</summary>
    public UpperStructureResult(IReadOnlyList<UpperStructureHit> hits, double epsilon, double? unknownFromFeet,
        string limitation) {
        MeasuredHits = hits.ToList().AsReadOnly();
        double nearest = hits.Count > 0 ? hits.Min(hit => hit.DistanceFeet) : double.PositiveInfinity;
        if(unknownFromFeet.HasValue && unknownFromFeet.Value <= nearest + epsilon) {
            State = UpperStructureState.Unavailable;
            Limitation = limitation;
            return;
        }
        Nearest = hits.Where(hit => hit.DistanceFeet <= nearest + epsilon).ToList().AsReadOnly();
        if(Nearest.Count > 0) {
            DistanceMm = nearest * SystemPluginConfig.MillimetersPerFoot;
            State = Nearest.Count == 1 ? UpperStructureState.Found : UpperStructureState.Ambiguous;
        } else {
            State = UpperStructureState.Absent;
        }
    }

    public UpperStructureState State { get; }
    public double? DistanceMm { get; }
    public IReadOnlyList<UpperStructureHit> MeasuredHits { get; }
    public IReadOnlyList<UpperStructureHit> Nearest { get; } = Array.Empty<UpperStructureHit>();
    public string Limitation { get; }
}

internal sealed partial class ManagerProjectSnapshot {
    /// <summary>Добавляет геометрические факты для проёмов опубликованных строк, включая партнёров вне диапазона.</summary>
    public ManagerProjectSnapshot(IEnumerable<ManagerOpeningSnapshot> openings, IEnumerable<ManagerLintelSnapshot> lintels,
        IEnumerable<ManagerWallSnapshot> walls, IEnumerable<ManagerStructureSnapshot> structures, int unreadableCount,
        IEnumerable<OpeningLintelMatch> matches, IEnumerable<AboveOpeningFacts> aboveOpenings)
        : this(openings, lintels, walls, structures, unreadableCount, matches) {
        AboveOpenings = new ReadOnlyDictionary<string, AboveOpeningFacts>(aboveOpenings.ToDictionary(f => f.OpeningId));
    }

    public IReadOnlyDictionary<string, AboveOpeningFacts> AboveOpenings { get; } =
        new ReadOnlyDictionary<string, AboveOpeningFacts>(new Dictionary<string, AboveOpeningFacts>());
}
