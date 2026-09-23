using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningTaskIncomingStatus"/> вычисляются
/// </summary>
internal class IncomingTaskStatusesSettings {
    /// <summary>
    /// Включает проверку расположения задания в недопустимых конструкциях.
    /// <para>Выполняет отдельный поиск пересечений, от остальных проверок не зависит.</para>
    /// </summary>
    public bool CheckUnacceptableConstructions { get; set; } = true;

    /// <summary>
    /// Включает сопоставление задания с конструкциями и чистовыми отверстиями активного файла.
    /// <para>
    /// Родительская проверка: выполняет сбор пересечений, на котором основаны
    /// <see cref="CheckDifferentConstructions"/> и <see cref="CheckHost"/>.
    /// </para>
    /// </summary>
    public bool CheckIntersections { get; set; } = true;

    /// <summary>
    /// Включает проверку расположения задания в конструкциях разных категорий.
    /// <para>Работает только вместе с <see cref="CheckIntersections"/>, см. <see cref="DifferentConstructionsEnabled"/>.</para>
    /// </summary>
    public bool CheckDifferentConstructions { get; set; } = true;

    /// <summary>
    /// Включает поиск основы задания на отверстие.
    /// <para>Работает только вместе с <see cref="CheckIntersections"/>, см. <see cref="HostEnabled"/>.</para>
    /// </summary>
    public bool CheckHost { get; set; } = true;

    /// <summary>
    /// Выполняется ли проверка расположения задания в конструкциях разных категорий
    /// </summary>
    [JsonIgnore]
    public bool DifferentConstructionsEnabled => CheckIntersections && CheckDifferentConstructions;

    /// <summary>
    /// Выполняется ли поиск основы задания на отверстие
    /// </summary>
    [JsonIgnore]
    public bool HostEnabled => CheckIntersections && CheckHost;
}
