namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы Навигатора вычисляются в файле КР
/// </summary>
internal class NavigatorKrSettings {
    /// <summary>
    /// Настройки статусов входящих в файл КР заданий на отверстия (от АР или от ВИС)
    /// </summary>
    public IncomingTaskStatusesSettings IncomingTaskSettings { get; set; } = new();

    /// <summary>
    /// Настройки статусов чистовых отверстий КР
    /// </summary>
    public RealOpeningKrStatusesSettings RealOpeningSettings { get; set; } = new() {
        TooBigVolumeRatio = 0.5
    };
}
