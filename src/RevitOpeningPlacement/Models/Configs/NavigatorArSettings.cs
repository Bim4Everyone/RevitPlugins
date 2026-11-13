namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы Навигатора вычисляются в файле АР
/// </summary>
internal class NavigatorArSettings {
    /// <summary>
    /// Настройки статусов входящих в файл АР заданий на отверстия
    /// </summary>
    public IncomingTaskStatusesSettings IncomingTaskSettings { get; set; } = new();

    /// <summary>
    /// Настройки статусов чистовых отверстий АР
    /// </summary>
    public RealOpeningArStatusesSettings RealOpeningSettings { get; set; } = new();
}
