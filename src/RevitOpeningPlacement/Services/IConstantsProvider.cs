namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис с константами, которые использует плагин
/// </summary>
public interface IConstantsProvider {
    /// <summary>
    /// Точность для определения расстояний в единицах Revit
    /// </summary>
    double ToleranceDistanceFeet { get; }

    /// <summary>
    /// Точность для определения объемов в единицах Revit
    /// </summary>
    double ToleranceVolumeFeetCube { get; }

    /// <summary>
    /// Процент толерантности объемов солидов в долях от 1
    /// </summary>
    double ToleranceVolumePercentage { get; }

    /// <summary>
    /// Минимальное значение габарита задания на отверстие в футах
    /// </summary>
    double OpeningTaskSizeMinValueFeet { get; }

    /// <summary>
    /// Укрупненное значение шага прогресса
    /// </summary>
    int ProgressBarStepLarge { get; }

    /// <summary>
    /// Уменьшенное значение шага прогресса
    /// </summary>
    int ProgressBarStepSmall { get; }
}
