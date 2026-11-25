using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Технический результат поиска, независимый от статусов и действий менеджера.</summary>
internal enum MatchState { Confirmed, Shifted, NoCandidate, OneWay, Ambiguous, Incomplete }

/// <summary>Подтверждённая причина конфликта проёмов в несопоставленной строке.</summary>
internal enum OpeningConflict { None, IntersectingCuts, CompetingOpenings }

/// <summary>Концы существующего семейства, подтверждённые в примыкающей стене вне перечня ЖБ.</summary>
[Flags]
internal enum RetainedLintelEnds { None = 0, Left = 1, Right = 2 }

/// <summary>Измерения для двух конкретных наборов плоскостей.</summary>
internal sealed class MatchContact(string openingId, string lintelId, PlaneDeviation deviation) {
    public string OpeningId { get; } = openingId;
    public string LintelId { get; } = lintelId;
    public PlaneDeviation Deviation { get; } = deviation;
}

/// <summary>Одна строка диапазона. Два снимка вместе публикуются при взаимно однозначной связи.</summary>
internal sealed class OpeningLintelMatch(ManagerOpeningSnapshot opening, ManagerLintelSnapshot lintel,
    MatchState state, IEnumerable<MatchContact> contacts, string limitation = null) {
    /// <summary>Добавляет конкретную причину конфликта без изменения результата поиска.</summary>
    public OpeningLintelMatch(ManagerOpeningSnapshot opening, ManagerLintelSnapshot lintel, MatchState state,
        IEnumerable<MatchContact> contacts, string limitation, OpeningConflict conflict)
        : this(opening, lintel, state, contacts, limitation) {
        Conflict = conflict;
    }

    /// <summary>Добавляет независимый результат чтения замка уже сопоставленной пары.</summary>
    public OpeningLintelMatch(OpeningLintelMatch pair, LintelLockFacts lockFacts)
        : this(pair.Opening, pair.Lintel, pair.State, pair.Contacts, pair.Limitation, pair.Conflict) {
        LockFacts = lockFacts;
        RetainedEnds = pair.RetainedEnds;
    }

    /// <summary>Добавляет признаки сохранения каждого фактического конца отдельно.</summary>
    public OpeningLintelMatch(OpeningLintelMatch pair, RetainedLintelEnds retainedEnds)
        : this(pair, pair.LockFacts) {
        RetainedEnds = retainedEnds;
    }

    public ManagerOpeningSnapshot Opening { get; } = opening;
    public ManagerLintelSnapshot Lintel { get; } = lintel;
    public MatchState State { get; } = state;
    public IReadOnlyList<MatchContact> Contacts { get; } = contacts.ToList().AsReadOnly();
    public string Limitation { get; } = limitation;
    public OpeningConflict Conflict { get; }
    public LintelLockFacts LockFacts { get; }
    public RetainedLintelEnds RetainedEnds { get; }
}
