using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Сопоставляет числовые плоскости одного поколения анализа в координатах исходного документа.</summary>
internal static class OpeningLintelMatcher {
    /// <summary>Ищет пары в пределах трёх локальных отклонений и ограничивает только выдачу строк.</summary>
    public static IReadOnlyList<OpeningLintelMatch> Match(IReadOnlyList<ManagerOpeningSnapshot> openings,
        IReadOnlyList<ManagerLintelSnapshot> lintels, ISet<string> scope, MatchLimits limits, double epsilon,
        bool catalogIncomplete, Func<string, GeometryBounds> hostBounds) {
        var graph = new OpeningLintelGraph(openings, lintels, epsilon);
        if(catalogIncomplete) {
            return graph.Resolve(scope, true);
        }
        var index = new MatchColumns<ManagerLintelSnapshot>(lintels.Where(l => l.Geometry?.Anchor is not null),
            l => l.Geometry.Anchor.Center);
        // Сфера вокруг куба трёх локальных допусков охватывает его при любом повороте стены.
        double margin = limits.SearchMm / SystemPluginConfig.MillimetersPerFoot * Math.Sqrt(3) + epsilon;
        bool unknownAnchor = false;
        foreach(var lintel in lintels.Where(l => l.Geometry?.Anchor is null)) {
            graph.MarkIncomplete(lintel.Element.UniqueId, lintel.Geometry?.Limitation ?? "Недоступны плоскости перемычки.");
            unknownAnchor = true;
        }
        foreach(var opening in openings) {
            string id = opening.Element.UniqueId;
            var geometry = opening.Geometry;
            var target = geometry?.Cut?.TargetPlanes;
            if(target is null) {
                continue;
            }
            if(unknownAnchor) {
                graph.MarkIncomplete(id, "Часть привязок перемычек не удалось локализовать.");
            }
            var center = target.Center;
            foreach(var lintel in index.Within(new GeometryBounds(center, center), margin)) {
                var actual = lintel.Geometry.Anchor;
                if(Math.Abs(actual.Center.Z - center.Z) <= margin) {
                    var deviation = new PlaneDeviation(target, actual, limits);
                    if(deviation.InRange) {
                        graph.Record(id, lintel.Element.UniqueId, deviation);
                    }
                }
            }
        }

        foreach(var opening in openings.Where(o => o.Geometry?.Cut is null)) {
            var possible = FindPotentialLintels(opening, lintels, index, limits, margin, hostBounds);
            bool incomplete = unknownAnchor || opening.Geometry?.OpenTop is null;
            bool hasLocalizedConflict = opening.Geometry is { HasConflictingCut: true, SearchTopCenter: not null,
                Wall: not null };
            foreach(var lintel in possible) {
                incomplete = true;
                graph.MarkIncomplete(lintel.Element.UniqueId, "Не удалось исключить проём с неподтверждённым вырезом.",
                    hasLocalizedConflict);
            }
            if(incomplete) {
                graph.MarkIncomplete(opening.Element.UniqueId, opening.Geometry?.Limitation
                    ?? "Недоступны плоскости выреза стены.", opening.Geometry?.HasConflictingCut == true);
            }
        }
        return graph.Resolve(scope, false);
    }

    /// <summary>Ищет среди раздельных строк кандидатов для проверки прямой связи центральных плоскостей.</summary>
    internal static IEnumerable<OpeningLintelMatch> FindLockCandidates(IReadOnlyList<OpeningLintelMatch> rows,
        MatchLimits limits, double epsilon) {
        var lintels = rows.Where(row => row.Opening is null && row.State == MatchState.Incomplete
            && row.Lintel?.Geometry?.Anchor is not null).Select(row => row.Lintel);
        var index = new MatchColumns<ManagerLintelSnapshot>(lintels, lintel => lintel.Geometry.Anchor.Center);
        double margin = limits.AlignmentMm / SystemPluginConfig.MillimetersPerFoot * Math.Sqrt(3) + epsilon;
        foreach(var row in rows.Where(row => row.Lintel is null && row.State == MatchState.Incomplete)) {
            if(row.Opening.Geometry is not { Cut: null, SearchPlanes: { } target }) {
                continue;
            }
            var bounds = new GeometryBounds(target.Center, target.Center);
            foreach(var lintel in index.Within(bounds, margin)) {
                var deviation = new PlaneDeviation(target, lintel.Geometry.Anchor, limits);
                if(deviation.InRange && !deviation.IsShifted) {
                    var contact = new MatchContact(row.Opening.Element.UniqueId, lintel.Element.UniqueId, deviation);
                    yield return new OpeningLintelMatch(row.Opening, lintel, MatchState.Confirmed, [contact],
                        row.Limitation, row.Conflict);
                }
            }
        }
    }

    // При неизвестном центре верха ограничивает поиск габаритами стены и известной нижней гранью проёма.
    private static IEnumerable<ManagerLintelSnapshot> FindPotentialLintels(ManagerOpeningSnapshot opening,
        IReadOnlyList<ManagerLintelSnapshot> lintels, MatchColumns<ManagerLintelSnapshot> index, MatchLimits limits,
        double margin, Func<string, GeometryBounds> hostBounds) {
        var geometry = opening.Geometry;
        if(geometry?.SearchPlanes is { } target) {
            var center = target.Center;
            return index.Within(new GeometryBounds(center, center), margin)
                .Where(lintel => new PlaneDeviation(target, lintel.Geometry.Anchor, limits).InRange);
        }

        var bounds = opening.CutWallUniqueId is { } host ? hostBounds(host) : null;
        var candidates = bounds is null ? lintels : index.Within(bounds, margin).Where(lintel => {
            var location = lintel.Geometry.Anchor.Center;
            return bounds.Overlaps(location, location, margin);
        });
        if(geometry?.LowestSillElevation is not { } sill) {
            return candidates;
        }
        double heightTolerance = (limits.SearchMm + SystemPluginConfig.MatchRoundoffMm)
            / SystemPluginConfig.MillimetersPerFoot;
        return candidates.Where(lintel => lintel.Geometry?.Anchor is not { } anchor
            || anchor.Center.Z + heightTolerance >= sill);
    }
}
