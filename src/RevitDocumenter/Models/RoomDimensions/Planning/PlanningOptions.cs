namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Параметры планирования размеров для одного помещения. Все значения в футах модели.
/// </summary>
internal class PlanningOptions {
    /// <summary>
    /// Отступ размерной линии от контура.
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// Добавка к отступу, если текст размера обращен к контуру.
    /// </summary>
    public double TextAllowance { get; set; }

    /// <summary>
    /// Допуск совпадения плоскостей привязки и равенства значений.
    /// </summary>
    public double CoordinateTolerance { get; set; }

    /// <summary>
    /// Максимальный сдвиг размерной линии при объединении в цепочку.
    /// </summary>
    public double MaxShift { get; set; }

    /// <summary>
    /// Максимальная длина выноски от контура до размерной линии цепочки.
    /// </summary>
    public double MaxLeader { get; set; }

    /// <summary>
    /// Шаг между ярусами размеров.
    /// </summary>
    public double TierStep { get; set; }

    /// <summary>
    /// Общий размер ставится, если в самой длинной цепочке вдоль оси больше этого числа сегментов.
    /// </summary>
    public int OverallMinSegments { get; set; }
}
