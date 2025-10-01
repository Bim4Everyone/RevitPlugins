namespace RevitOpeningPlacement.Services;
internal class ConstantsProvider : IConstantsProvider {
    /// <summary>
    /// Точность для определения расстояний и координат 1 мм в футах
    /// </summary>
    private const double _toleranceDistance = 1 / 304.8;

    /// <summary>
    /// Точность для определения объемов 1 см3 в футах
    /// </summary>
    private const double _toleranceVolume = 10 / 304.8 * (10 / 304.8) * (10 / 304.8);

    /// <summary>
    /// Минимальное значение габарита задания на отверстие в футах (~5 мм)
    /// </summary>
    private const double _minGeometryFeetSize = 0.015;

    /// <summary>
    /// Процент толерантности объемов солидов
    /// </summary>
    private const double _toleranceVolumePercentage = 0.01;

    private const int _progressBarStepLarge = 100;

    private const int _progressBarStepSmall = 25;


    public double ToleranceDistanceFeet => _toleranceDistance;

    public double ToleranceVolumeFeetCube => _toleranceVolume;

    public double ToleranceVolumePercentage => _toleranceVolumePercentage;

    public double OpeningTaskSizeMinValueFeet => _minGeometryFeetSize;

    public int ProgressBarStepLarge => _progressBarStepLarge;

    public int ProgressBarStepSmall => _progressBarStepSmall;
}
