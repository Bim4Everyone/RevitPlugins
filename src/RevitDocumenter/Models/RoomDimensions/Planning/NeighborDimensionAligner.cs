using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Contours;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Выравнивает соседние размерные линии через общую границу помещений перед созданием размеров.
/// </summary>
internal static class NeighborDimensionAligner {
    private sealed class Candidate {
        public Candidate(RoomRunResult room, ContourDimensionPlanner.RoomShape shape, VirtualDimension dimension) {
            Room = room;
            Shape = shape;
            Dimension = dimension;
            StartBoundary = GetBoundarySource(room.Contour, dimension.Anchors[0]);
            EndBoundary = GetBoundarySource(room.Contour, dimension.Anchors[dimension.Anchors.Count - 1]);
        }

        public RoomRunResult Room { get; }
        public ContourDimensionPlanner.RoomShape Shape { get; }
        public VirtualDimension Dimension { get; }
        public ElementId StartBoundary { get; }
        public ElementId EndBoundary { get; }
    }

    private sealed class Layout {
        public Layout(Candidate candidate, List<string> log = null) {
            Candidate = candidate;
            Position = candidate.Dimension.LinePosition;
            Offsets = new Dictionary<int, TextOffset>(candidate.Dimension.TextOffsets);
            Log = log;
        }

        public Candidate Candidate { get; }
        public double Position { get; }
        public Dictionary<int, TextOffset> Offsets { get; }
        public List<string> Log { get; }

        public void Restore() {
            var dimension = Candidate.Dimension;
            dimension.LinePosition = Position;
            dimension.TextOffsets.Clear();
            foreach(var offset in Offsets) {
                dimension.TextOffsets.Add(offset.Key, offset.Value);
            }
        }
    }

    private const int _maxGroupSize = 4;
    private const double _directionTolerance = 1e-9;
    private const double _distanceTolerance = 1e-7;
    private static readonly double _maxGap = UnitUtilsHelper.ConvertToInternalValue(500);

    public static void Align(IEnumerable<RoomRunResult> rooms) {
        var planned = rooms.Where(r => r.PlanningOptions != null).ToList();
        var candidates = planned.SelectMany(GetCandidates).OrderBy(c => c.Dimension.Min).ToList();
        var byStart = candidates.Where(c => c.StartBoundary != null).ToLookup(c => c.StartBoundary);
        var used = new HashSet<Candidate>();
        var viewOptions = planned.SelectMany(r => r.Dimensions.Select(d => (Dimension: d, r.PlanningOptions)))
            .ToDictionary(x => x.Dimension, x => x.PlanningOptions);
        foreach(var first in candidates.Where(c => !used.Contains(c))) {
            var group = new List<Candidate> { first };
            while(group.Count < _maxGroupSize && group[group.Count - 1].EndBoundary is ElementId boundary) {
                var next = byStart[boundary].Where(c => !used.Contains(c) && CanFollow(group, c))
                    .OrderBy(c => Math.Abs(c.Dimension.LinePosition - group[group.Count - 1].Dimension.LinePosition))
                    .ThenBy(c => c.Dimension.Min)
                    .FirstOrDefault();
                if(next is null) {
                    break;
                }
                group.Add(next);
            }

            while(group.Count > 1) {
                if(TryAlign(group, viewOptions)) {
                    used.UnionWith(group);
                    break;
                }
                group.RemoveAt(group.Count - 1);
            }
        }
    }

    private static IEnumerable<Candidate> GetCandidates(RoomRunResult room) {
        var shape = new ContourDimensionPlanner.RoomShape(room.Contour);
        return room.Dimensions.Where(d => !d.InvolvesInner && (d.Kind == "Contour" || d.Kind == "Chain"))
            .Select(d => new Candidate(room, shape, d))
            .Where(c => c.StartBoundary != null || c.EndBoundary != null);
    }

    private static ElementId GetBoundarySource(RoomContour contour, DimensionAnchor anchor) {
        var element = contour.Outer?.Elements.FirstOrDefault(e => e.Index == anchor.ElementIndex);
        if(contour.Outer?.Index != anchor.LoopIndex || element is not ContourLine line
           || line.SourceId == ElementId.InvalidElementId
           || (line.LinkSourceId != null && line.LinkSourceId != ElementId.InvalidElementId)) {
            return null;
        }
        return line.SourceId;
    }

    private static bool CanFollow(List<Candidate> group, Candidate next) {
        var last = group[group.Count - 1];
        var a = last.Dimension;
        var b = next.Dimension;
        double tolerance = next.Room.PlanningOptions.CoordinateTolerance;
        double gap = b.Min - a.Max;
        return gap >= -_distanceTolerance && gap <= _maxGap + _distanceTolerance && a.SideSign == b.SideSign
               && a.Direction.DistanceTo(b.Direction) <= _directionTolerance
               && group.All(c => c.Room != next.Room
                                 && Math.Abs(c.Dimension.Z - b.Z) <= tolerance
                                 && Math.Abs(c.Dimension.LinePosition - b.LinePosition) <= _maxGap + _distanceTolerance);
    }

    private static bool TryAlign(List<Candidate> group, Dictionary<VirtualDimension, PlanningOptions> viewOptions) {
        var original = group.Select(c => new Layout(c)).ToList();
        double min = original.Min(s => s.Position);
        double max = original.Max(s => s.Position);
        double tolerance = group.Min(c => c.Room.PlanningOptions.CoordinateTolerance);
        if(max - min <= tolerance) {
            return false;
        }

        var positions = original.Select(s => s.Position).Distinct().OrderBy(p => p).ToList();
        var midpoints = positions.Zip(positions.Skip(1), (a, b) => (a + b) / 2).ToList();
        double step = group.Min(c => c.Room.PlanningOptions.TextSize) / 4;
        positions.AddRange(midpoints);
        if(step > tolerance) {
            positions.AddRange([min - step, max + step, min - 2 * step, max + 2 * step]);
        }

        var initialProblems = group.ToDictionary(c => c, GetProblems);
        List<Layout> best = null;
        (int TextChanges, int Moved, double Distance) bestScore = (int.MaxValue, int.MaxValue, double.MaxValue);
        foreach(double position in positions.Where(p => max - p <= _maxGap + _distanceTolerance
                                                        && p - min <= _maxGap + _distanceTolerance)) {
            var moved = original.Where(s => Math.Abs(s.Position - position) > _distanceTolerance).ToList();
            if(moved.Any(s => !IsValidPosition(s.Candidate, position))) {
                continue;
            }
            try {
                var trial = moved.Select(s => Place(s.Candidate, position)).ToList();
                if(moved.Any(s => !GetProblems(s.Candidate).IsSubsetOf(initialProblems[s.Candidate])
                                  || HasViewConflict(s.Candidate, viewOptions))) {
                    continue;
                }
                var score = (trial.Zip(moved, (now, before) => CountTextChanges(before, now, tolerance)).Sum(),
                    moved.Count, moved.Sum(s => Math.Abs(s.Position - position)));
                if(score.CompareTo(bestScore) < 0) {
                    bestScore = score;
                    best = trial;
                }
            } finally {
                foreach(var layout in moved) {
                    layout.Restore();
                }
            }
        }
        if(best is null) {
            return false;
        }

        foreach(var layout in best) {
            var candidate = layout.Candidate;
            double shift = UnitUtilsHelper.ConvertFromInternalValue(layout.Position - candidate.Dimension.LinePosition);
            layout.Restore();
            candidate.Room.PlanningLog.Add(
                $"Выравнивание соседних размеров [{string.Join(", ", candidate.Dimension.Sources)}]: "
                + $"сдвиг {shift:0.#} мм, линий в группе {group.Count}");
            candidate.Room.PlanningLog.AddRange(layout.Log);
        }
        return true;
    }

    private static bool IsValidPosition(Candidate candidate, double position) {
        var room = candidate.Room;
        return candidate.Dimension.Anchors.All(a => a.WitnessLength(position)
                                                    <= room.PlanningOptions.MaxWitnessLine
                                                    + room.PlanningOptions.CoordinateTolerance)
               && ContourDimensionPlanner.IsLocalChainPositionValid(
                   candidate.Shape, candidate.Dimension, position, room.Dimensions, room.PlanningOptions);
    }

    private static Layout Place(Candidate candidate, double position) {
        var log = new List<string>();
        candidate.Dimension.LinePosition = position;
        TextConflictCounter.Place(candidate.Room.Dimensions, candidate.Shape.ContainsShape, candidate.Shape.Segments,
            candidate.Room.PlanningOptions, log, new HashSet<VirtualDimension> { candidate.Dimension });
        return new Layout(candidate, log);
    }

    private static HashSet<(VirtualDimension Dimension, int Segment)> GetProblems(Candidate candidate) {
        return TextConflictCounter.GetLayoutProblems(
            candidate.Room.Dimensions, candidate.Shape.ContainsShape, candidate.Room.PlanningOptions);
    }

    private static bool HasViewConflict(Candidate candidate, Dictionary<VirtualDimension, PlanningOptions> viewOptions) {
        var others = viewOptions.Keys.Except(candidate.Room.Dimensions);
        return TextConflictCounter.FindConflict(candidate.Dimension, candidate.Dimension.LinePosition, others,
            candidate.Room.PlanningOptions, d => viewOptions[d]) != null;
    }

    private static int CountTextChanges(Layout before, Layout after, double tolerance) {
        return before.Offsets.Keys.Union(after.Offsets.Keys).Count(key =>
            !before.Offsets.TryGetValue(key, out var a) || !after.Offsets.TryGetValue(key, out var b)
            || a.Leader != b.Leader || Math.Abs(a.Along - b.Along) > tolerance
            || Math.Abs(a.Across - b.Across) > tolerance);
    }
}
