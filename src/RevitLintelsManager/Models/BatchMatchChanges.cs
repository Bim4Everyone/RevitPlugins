using System;
using System.Collections.Generic;
using System.Linq;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Учитывает только сохранённые изменения привязок между строками одного пакета.</summary>
internal sealed class BatchMatchChanges {
    private readonly Dictionary<string, ManagerLintelSnapshot> _changes = [];
    private readonly HashSet<string> _affectedOpenings = [];
    private HashSet<(string Opening, string Lintel, MatchState State)> _currentPairs;

    /// <summary>Отмечает проёмы, для которых старая или новая привязка могла изменить выбор пары.</summary>
    internal void Record(ProcessingReadData data, ManagerLintelSnapshot previous, ManagerLintelSnapshot current,
        MatchLimits limits) {
        if(previous is not null) {
            _changes[previous.Element.UniqueId] = null;
        }
        if(current is not null) {
            _changes[current.Element.UniqueId] = current;
        }
        _currentPairs = null;
        foreach(var anchor in new[] { previous?.Geometry?.Anchor, current?.Geometry?.Anchor }.Where(a => a is not null)) {
            double margin = limits.SearchMm / SystemPluginConfig.MillimetersPerFoot * Math.Sqrt(3)
                + SystemPluginConfig.GeometryLengthEpsilonFeet;
            foreach(var opening in data.OpeningCenters.Within(new GeometryBounds(anchor.Center, anchor.Center), margin)) {
                if(new PlaneDeviation(opening.Geometry.Cut.TargetPlanes, anchor, limits).InRange) {
                    _affectedOpenings.Add(opening.Element.UniqueId);
                }
            }
        }
    }

    /// <summary>При пересечении зон поиска проверяет взаимность по числовым снимкам, включая соседей вне диапазона.</summary>
    internal bool PreservesPair(ProcessingReadData data, OpeningLintelMatch match, MatchLimits limits) {
        if(!_affectedOpenings.Contains(match.Opening?.Element.UniqueId)
            && !match.Contacts.Any(contact => _affectedOpenings.Contains(contact.OpeningId))) {
            return true;
        }
        if(_currentPairs is null) {
            var lintels = data.MatchLintels.Where(lintel => !_changes.ContainsKey(lintel.Element.UniqueId))
                .Concat(_changes.Values.Where(lintel => lintel is not null)).ToList();
            _currentPairs = OpeningLintelMatcher.Match(data.MatchOpenings, lintels, null, limits,
                    SystemPluginConfig.GeometryLengthEpsilonFeet, false,
                    id => data.MatchHostBounds.TryGetValue(id, out var bounds) ? bounds : null)
                .Select(row => (row.Opening?.Element.UniqueId, row.Lintel?.Element.UniqueId, row.State)).ToHashSet();
        }
        return _currentPairs.Contains((match.Opening?.Element.UniqueId, match.Lintel?.Element.UniqueId, match.State));
    }
}
