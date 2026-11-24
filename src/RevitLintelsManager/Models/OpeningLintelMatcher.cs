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
            var target = opening.Geometry?.Cut?.TargetPlanes;
            if(target is null) {
                graph.MarkIncomplete(id, opening.Geometry?.Limitation ?? "Недоступны плоскости выреза стены.");
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

        // У неизвестного выреза исключаем только те перемычки, чья привязка заведомо вне рамки его стены.
        foreach(var group in openings.Where(o => o.Geometry?.Cut is null).GroupBy(o => o.HostWallUniqueId)) {
            var bounds = group.Key is null ? null : hostBounds(group.Key);
            var possible = bounds is null ? lintels : index.Within(bounds, margin)
                .Where(l => l.Geometry.Anchor.Center.Z >= bounds.Min.Z - margin
                    && l.Geometry.Anchor.Center.Z <= bounds.Max.Z + margin);
            foreach(var lintel in possible) {
                graph.MarkIncomplete(lintel.Element.UniqueId, "Не удалось исключить проём с неподтверждённым вырезом.");
            }
        }
        return graph.Resolve(scope, false);
    }
}
