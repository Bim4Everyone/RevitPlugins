using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Разрешает конкуренцию за пару по всему документу до проекции результата на диапазон менеджера.</summary>
internal sealed class OpeningLintelGraph {
    private sealed class Vertex(ManagerOpeningSnapshot opening, ManagerLintelSnapshot lintel) {
        public ManagerOpeningSnapshot Opening { get; } = opening;
        public ManagerLintelSnapshot Lintel { get; } = lintel;
        public string Id => Opening?.Element.UniqueId ?? Lintel.Element.UniqueId;
        public List<MatchContact> Contacts { get; } = [];
        public MatchContact Best { get; set; }
        public int NearestCount { get; set; }
        public int ClaimCount { get; set; }
        public string Limitation { get; set; }
    }

    private readonly Dictionary<string, Vertex> _vertices;
    private readonly double _epsilon;

    /// <summary>Регистрирует обе стороны независимо от выбранного диапазона.</summary>
    public OpeningLintelGraph(IEnumerable<ManagerOpeningSnapshot> openings, IEnumerable<ManagerLintelSnapshot> lintels,
        double epsilon) {
        _vertices = openings.Select(o => new Vertex(o, null)).Concat(lintels.Select(l => new Vertex(null, l)))
            .ToDictionary(v => v.Id);
        _epsilon = epsilon;
    }

    /// <summary>Одно сравнение плоскостей служит общей мерой для независимого выбора с обеих сторон.</summary>
    public void Record(string opening, string lintel, PlaneDeviation deviation) {
        var contact = new MatchContact(opening, lintel, deviation);
        _vertices[opening].Contacts.Add(contact);
        _vertices[lintel].Contacts.Add(contact);
    }

    /// <summary>Сохраняет первую установленную причину неполноты поиска.</summary>
    public void MarkIncomplete(string id, string reason) {
        _vertices[id].Limitation ??= reason;
    }

    /// <summary>Строит строки диапазона с обоими участниками подтверждённых и смещённых пар.</summary>
    public IReadOnlyList<OpeningLintelMatch> Resolve(ISet<string> scope, bool catalogIncomplete) {
        foreach(var vertex in _vertices.Values) {
            vertex.Best = null;
            vertex.NearestCount = 0;
            vertex.ClaimCount = 0;
        }
        foreach(var vertex in _vertices.Values) {
            CountClaims(vertex);
        }

        var rows = new List<OpeningLintelMatch>();
        var emitted = new HashSet<string>();
        foreach(var vertex in _vertices.Values) {
            if((scope is not null && !scope.Contains(vertex.Id)) || !emitted.Add(vertex.Id)) {
                continue;
            }
            var state = Classify(vertex, catalogIncomplete, out string reason);
            if(state is MatchState.Confirmed or MatchState.Shifted) {
                var partner = Other(vertex, vertex.Best);
                emitted.Add(partner.Id);
                rows.Add(new OpeningLintelMatch(vertex.Opening ?? partner.Opening, vertex.Lintel ?? partner.Lintel,
                    state, vertex.Contacts.Union(partner.Contacts)));
            } else {
                rows.Add(new OpeningLintelMatch(vertex.Opening, vertex.Lintel, state, vertex.Contacts, reason));
            }
        }
        return rows.AsReadOnly();
    }

    // Предпочитает совпавшие плоскости, затем ближайшую привязку. Все равноудалённые кандидаты получают претензию.
    private void CountClaims(Vertex source) {
        if(source.Contacts.Count == 0) {
            return;
        }
        bool shifted = source.Contacts.All(c => c.Deviation.IsShifted);
        var candidates = source.Contacts.Where(c => c.Deviation.IsShifted == shifted);
        double minimum = candidates.Min(c => c.Deviation.Distance);
        foreach(var contact in candidates.Where(c => c.Deviation.Distance <= minimum + _epsilon)) {
            source.Best = contact;
            source.NearestCount++;
            Other(source, contact).ClaimCount++;
        }
    }

    // Проверяет взаимность и число претендентов до объединения элементов в одну строку.
    private MatchState Classify(Vertex source, bool catalogIncomplete, out string reason) {
        reason = source.Limitation;
        if(catalogIncomplete || reason is not null) {
            reason ??= "Каталог экземпляров прочитан частично.";
            return MatchState.Incomplete;
        }
        if(source.Best is null) {
            return MatchState.NoCandidate;
        }
        var partner = Other(source, source.Best);
        if(source.NearestCount > 1 || source.ClaimCount > 1 || partner.NearestCount > 1 || partner.ClaimCount > 1) {
            return MatchState.Ambiguous;
        }
        if(partner.Limitation is not null) {
            reason = partner.Limitation;
            return MatchState.Incomplete;
        }
        if(partner.Best != source.Best) {
            return MatchState.OneWay;
        }
        return source.Best.Deviation.IsShifted ? MatchState.Shifted : MatchState.Confirmed;
    }

    // Возвращает второй конец данного геометрического контакта.
    private Vertex Other(Vertex source, MatchContact contact) {
        return _vertices[source.Opening is null ? contact.OpeningId : contact.LintelId];
    }
}
