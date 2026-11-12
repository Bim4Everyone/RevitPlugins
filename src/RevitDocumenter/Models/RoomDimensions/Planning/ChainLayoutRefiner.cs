using System;
using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Ограниченный перенос цепочек после размещения текста (D75). Пробует одну цепочку за раз,
/// повторно размещает соседние тексты и принимает только уменьшение числа оставшихся проблем.
/// Не меняет привязки и состав размеров; без улучшения полностью восстанавливает прежнюю раскладку.
/// </summary>
internal static class ChainLayoutRefiner {
    /// <summary>
    /// Снимок изменяемой части плана; TextOffset неизменяем, копируется только словарь.
    /// </summary>
    private class DimensionState {
        public DimensionState(VirtualDimension dimension) {
            Dimension = dimension;
            Position = dimension.LinePosition;
            Offsets = new Dictionary<int, TextOffset>(dimension.TextOffsets);
        }

        public VirtualDimension Dimension { get; }
        public double Position { get; }
        public Dictionary<int, TextOffset> Offsets { get; }
    }

    // Не более двух проблемных цепочек, 32 кандидата на каждую, без каскадного переноса соседних линий.
    private const int _maxChains = 2;
    private const int _stepsPerSide = 16;
    private const double _stepTextRatio = 0.5;

    public static void Refine(
        List<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        IReadOnlyList<Segment2D> contour,
        Func<VirtualDimension, double, bool> validPosition,
        PlanningOptions options,
        List<string> log) {
        dimensions.ThrowIfNull();
        fitsRoom.ThrowIfNull();
        contour.ThrowIfNull();
        validPosition.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        double step = options.TextSize * _stepTextRatio;
        if(step <= options.CoordinateTolerance) {
            return;
        }
        var chains = TextConflictCounter.GetUnplacedChains(dimensions, fitsRoom, options).Take(_maxChains).ToList();
        foreach(var chain in chains) {
            if(!TextConflictCounter.GetUnplacedChains(dimensions, fitsRoom, options).Contains(chain)) {
                continue;
            }
            TryMoveChain(chain, dimensions, fitsRoom, contour, validPosition, options, step, log);
        }
    }

    private static void TryMoveChain(
        VirtualDimension chain,
        List<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        IReadOnlyList<Segment2D> contour,
        Func<VirtualDimension, double, bool> validPosition,
        PlanningOptions options,
        double step,
        List<string> log) {
        var original = Capture(dimensions);
        double start = chain.LinePosition;
        var initial = TextConflictCounter.GetLayoutScore(dimensions, fitsRoom, options);
        int bestProblems = initial.Problems;
        double bestCost = double.MaxValue;
        List<DimensionState> best = null;
        List<string> bestLog = null;
        int tried = 0;
        for(int k = 1; k <= _stepsPerSide; k++) {
            foreach(int sign in new[] { 1, -1 }) {
                Restore(original);
                double position = start + sign * k * step;
                if(!validPosition(chain, position)) {
                    continue;
                }
                tried++;
                var movable = TextConflictCounter.GetAffectedDimensions(chain, position, dimensions, options);
                chain.LinePosition = position;
                chain.TextOffsets.Clear();
                var trialLog = new List<string>();
                TextConflictCounter.Place(dimensions, fitsRoom, contour, options, trialLog, movable);
                var score = TextConflictCounter.GetLayoutScore(dimensions, fitsRoom, options);
                double cost = score.Cost + Math.Abs(position - start);
                if(score.Problems >= initial.Problems || score.Problems > bestProblems
                   || (score.Problems == bestProblems && cost >= bestCost)) {
                    continue;
                }
                bestProblems = score.Problems;
                bestCost = cost;
                best = Capture(dimensions);
                bestLog = trialLog;
            }
        }
        Restore(best ?? original);
        string name = "[" + string.Join(", ", chain.Sources) + "]";
        if(best is null) {
            log.Add($"Локальная раскладка {name}: проверено {tried} положений, улучшения нет; оставлена прежняя");
            return;
        }
        double shift = UnitUtilsHelper.ConvertFromInternalValue(chain.LinePosition - start);
        log.Add($"Локальная раскладка {name}: перенос цепочки на {shift:0.#} мм; "
                + $"проблем {initial.Problems} -> {bestProblems}; проверено {tried} положений");
        log.AddRange(bestLog.Select(line => "Локальная раскладка: " + line));
    }

    private static List<DimensionState> Capture(List<VirtualDimension> dimensions) {
        return dimensions.Select(d => new DimensionState(d)).ToList();
    }

    private static void Restore(List<DimensionState> states) {
        foreach(var state in states) {
            state.Dimension.LinePosition = state.Position;
            state.Dimension.TextOffsets.Clear();
            foreach(var pair in state.Offsets) {
                state.Dimension.TextOffsets.Add(pair.Key, pair.Value);
            }
        }
    }


}
