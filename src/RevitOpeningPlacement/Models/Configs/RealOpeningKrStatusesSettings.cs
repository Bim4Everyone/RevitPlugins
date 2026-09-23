namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningRealStatus"/>
/// вычисляются для чистовых отверстий КР
/// </summary>
internal class RealOpeningKrStatusesSettings : RealOpeningStatusesSettings {
    /// <summary>
    /// Максимально допустимое значение <see cref="MinDistanceBetweenOpenings"/> в мм
    /// </summary>
    public const int MaxDistanceBetweenOpenings = 1000;

    /// <summary>
    /// Включает проверку расстояния между чистовыми отверстиями КР
    /// </summary>
    public bool CheckTooClose { get; set; } = true;

    /// <summary>
    /// Минимальное расстояние между чистовыми отверстиями в мм
    /// </summary>
    public int MinDistanceBetweenOpenings { get; set; } = 50;
}
