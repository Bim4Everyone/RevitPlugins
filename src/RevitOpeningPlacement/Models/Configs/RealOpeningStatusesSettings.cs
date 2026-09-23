namespace RevitOpeningPlacement.Models.Configs;

/// <summary>
/// Настройки того, какие статусы из <see cref="OpeningModels.Enums.OpeningRealStatus"/> вычисляются
/// </summary>
internal class RealOpeningStatusesSettings {
    /// <summary>
    /// Минимально допустимое значение <see cref="EmptyVolumeRatio"/>
    /// </summary>
    public const double MinEmptyVolumeRatio = 0.01;

    /// <summary>
    /// Максимально допустимое значение <see cref="EmptyVolumeRatio"/>
    /// </summary>
    public const double MaxEmptyVolumeRatio = 0.05;

    /// <summary>
    /// Максимально допустимое значение <see cref="TooBigVolumeRatio"/>.
    /// <para>Снизу оно ограничено текущим значением <see cref="EmptyVolumeRatio"/>.</para>
    /// </summary>
    public const double MaxTooBigVolumeRatio = 0.5;

    /// <summary>
    /// Включает проверку актуальности чистового отверстия
    /// </summary>
    public bool CheckNotActual { get; set; } = true;

    /// <summary>
    /// Включает проверку соответствия объема чистового отверстия элементам из связей.
    /// <para>Дает статусы "пустое" и "слишком большое".</para>
    /// </summary>
    public bool CheckVolumeMatch { get; set; } = true;

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается пустым
    /// </summary>
    public double EmptyVolumeRatio { get; set; } = MinEmptyVolumeRatio;

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается слишком большим
    /// </summary>
    public double TooBigVolumeRatio { get; set; } = 0.2;
}
