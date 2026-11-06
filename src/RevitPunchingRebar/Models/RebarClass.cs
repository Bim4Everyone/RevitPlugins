namespace RevitPunchingRebar.Models;

/// <summary>
/// Класс арматуры.
/// </summary>
/// <remarks>
/// Значение перечисления — код металлопроката, который записывается в семейство каркаса.
/// </remarks>
internal enum RebarClass {
    /// <summary>
    /// Арматура класса А240.
    /// </summary>
    A240 = 240,

    /// <summary>
    /// Арматура класса А500С.
    /// </summary>
    A500C = 501
}
