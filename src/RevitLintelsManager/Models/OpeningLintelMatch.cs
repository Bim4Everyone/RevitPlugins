using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Технический результат поиска, независимый от статусов и действий менеджера.</summary>
internal enum MatchState { Confirmed, Shifted, NoCandidate, OneWay, Ambiguous, Incomplete }

/// <summary>Измерения для двух конкретных наборов плоскостей.</summary>
internal sealed class MatchContact(string openingId, string lintelId, PlaneDeviation deviation) {
    public string OpeningId { get; } = openingId;
    public string LintelId { get; } = lintelId;
    public PlaneDeviation Deviation { get; } = deviation;
}

/// <summary>Одна строка диапазона. Два снимка вместе публикуются при взаимно однозначной связи.</summary>
internal sealed class OpeningLintelMatch(ManagerOpeningSnapshot opening, ManagerLintelSnapshot lintel,
    MatchState state, IEnumerable<MatchContact> contacts, string limitation = null) {
    public ManagerOpeningSnapshot Opening { get; } = opening;
    public ManagerLintelSnapshot Lintel { get; } = lintel;
    public MatchState State { get; } = state;
    public IReadOnlyList<MatchContact> Contacts { get; } = contacts.ToList().AsReadOnly();
    public string Limitation { get; } = limitation;
}
