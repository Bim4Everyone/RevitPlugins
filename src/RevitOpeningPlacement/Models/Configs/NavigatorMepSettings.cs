using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningTaskOutcomingStatus"/> вычисляются
/// </summary>
internal class NavigatorMepSettings {
    /// <summary>
    /// Включает проверку того, что задание размещено вручную
    /// </summary>
    public bool CheckManuallyPlaced { get; set; } = true;

    /// <summary>
    /// Включает проверку актуальности задания.
    /// </summary>
    public bool CheckNotActual { get; set; } = true;

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// </summary>
    public bool CheckDifferentConstructions { get; set; } = true;

    /// <summary>
    /// Включает поиск основы задания на отверстие.
    /// <para>Работает быстрее при включенной <see cref="CheckNotActual"/>: она уже находит конструкции-кандидаты.</para>
    /// </summary>
    public bool CheckHost { get; set; } = true;

    /// <summary>
    /// Включает проверку расположения задания в недопустимых конструкциях
    /// </summary>
    public bool CheckUnacceptableConstructions { get; set; } = true;

    /// <summary>
    /// Включает проверку пересечения задания с другими заданиями
    /// </summary>
    public bool CheckIntersects { get; set; } = true;

    /// <summary>
    /// Включает проверку того, что задание объединенное
    /// </summary>
    public bool CheckUnited { get; set; } = true;

    /// <summary>
    /// Включает проверку отступов задания от элемента инженерных систем.
    /// </summary>
    public bool CheckOffsets { get; set; } = true;
}
