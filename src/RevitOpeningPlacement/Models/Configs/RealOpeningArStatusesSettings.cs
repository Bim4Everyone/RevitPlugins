namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningRealStatus"/> вычисляются
/// </summary>
internal class RealOpeningArStatusesSettings : IRealOpeningStatusesSettings {
    public double MinEmptyVolumeRatio => 0.01;

    public double MaxEmptyVolumeRatio => 0.05;

    public double MaxTooBigVolumeRatio => 0.5;

    public bool CheckNotActual { get; set; } = true;

    public bool CheckVolumeMatch { get; set; } = true;

    public double EmptyVolumeRatio { get; set; } = 0.01;

    public double TooBigVolumeRatio { get; set; } = 0.2;
}
