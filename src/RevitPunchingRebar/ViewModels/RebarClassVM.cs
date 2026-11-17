using RevitPunchingRebar.Models;

namespace RevitPunchingRebar.ViewModels;

/// <summary>
/// Класс арматуры для выбора в окне.
/// </summary>
internal class RebarClassVM {
    /// <summary>
    /// Создает элемент списка классов арматуры.
    /// </summary>
    /// <param name="value">Класс арматуры.</param>
    /// <param name="name">Отображаемое название класса.</param>
    public RebarClassVM(RebarClass value, string name) {
        Value = value;
        Name = name;
    }

    /// <summary>
    /// Класс арматуры.
    /// </summary>
    public RebarClass Value { get; }

    /// <summary>
    /// Отображаемое название класса.
    /// </summary>
    public string Name { get; }
}
