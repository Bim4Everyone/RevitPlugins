namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningTaskIncomingStatus"/> вычисляются
/// </summary>
internal class IncomingTaskStatusesSettings {
    /// <summary>
    /// Включает проверку расположения задания в недопустимых конструкциях.
    /// </summary>
    public bool CheckUnacceptableConstructions { get; set; } = true;

    /// <summary>
    /// Включает сопоставление задания с конструкциями и чистовыми отверстиями активного файла.
    /// </summary>
    public bool CheckIntersections { get; set; } = true;

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// </summary>
    public bool CheckDifferentConstructions { get; set; } = true;

    /// <summary>
    /// Включает поиск основы задания на отверстие.
    /// </summary>
    public bool CheckHost { get; set; } = true;
}
