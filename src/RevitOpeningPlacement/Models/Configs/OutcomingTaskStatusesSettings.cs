using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningTaskOutcomingStatus"/> вычисляются
/// </summary>
internal class OutcomingTaskStatusesSettings {
    /// <summary>
    /// Включает проверку того, что задание размещено вручную
    /// </summary>
    public bool CheckManuallyPlaced { get; set; } = true;

    /// <summary>
    /// Включает проверку актуальности задания.
    /// <para>
    /// Родительская проверка: выполняет поиск конструкций-основ из связей,
    /// на котором основана <see cref="CheckDifferentConstructions"/>.
    /// </para>
    /// </summary>
    public bool CheckNotActual { get; set; } = true;

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// <para>Работает только вместе с <see cref="CheckNotActual"/>, см. <see cref="DifferentConstructionsEnabled"/>.</para>
    /// </summary>
    public bool CheckDifferentConstructions { get; set; } = true;

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
    /// <para>Дает статусы "слишком маленькое" и "слишком большое".</para>
    /// </summary>
    public bool CheckOffsets { get; set; } = true;

    /// <summary>
    /// Выполняется ли проверка расположения задания в конструкциях разных категорий
    /// </summary>
    [JsonIgnore]
    public bool DifferentConstructionsEnabled => CheckNotActual && CheckDifferentConstructions;
}
