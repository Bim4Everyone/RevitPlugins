namespace RevitOpeningPlacement.Models.Configs;

internal interface IRealOpeningStatusesSettings {
    /// <summary>
    /// Минимально допустимое значение <see cref="EmptyVolumeRatio"/>
    /// </summary>
    double MinEmptyVolumeRatio { get; }

    /// <summary>
    /// Максимально допустимое значение <see cref="EmptyVolumeRatio"/>
    /// </summary>
    double MaxEmptyVolumeRatio { get; }

    /// <summary>
    /// Максимально допустимое значение <see cref="TooBigVolumeRatio"/>.
    /// </summary>
    double MaxTooBigVolumeRatio { get; }

    /// <summary>
    /// Включает проверку актуальности чистового отверстия
    /// </summary>
    bool CheckNotActual { get; set; }

    /// <summary>
    /// Включает проверку соответствия объема чистового отверстия элементам из связей.
    /// </summary>
    bool CheckVolumeMatch { get; set; }

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается пустым
    /// </summary>
    double EmptyVolumeRatio { get; set; }

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается слишком большим
    /// </summary>
    double TooBigVolumeRatio { get; set; }
}
