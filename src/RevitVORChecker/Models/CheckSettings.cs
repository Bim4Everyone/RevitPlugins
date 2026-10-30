using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models;

/// <summary>
/// Параметры запуска проверки.
/// </summary>
internal class CheckSettings {
    /// <summary>
    /// Папка с моделями КР и выгрузками ВОР.
    /// </summary>
    public string ModelsFolder { get; set; } = string.Empty;

    /// <summary>
    /// Классификатор видов работ; пусто — найти в папке с моделями.
    /// </summary>
    public string? ClassifierPath { get; set; }

    /// <summary>
    /// Допуск глубины пересечения конструкций, мм.
    /// </summary>
    public double ClashToleranceMm { get; set; } = 50;

    public bool TakeScreenshots { get; set; } = true;

    /// <summary>
    /// Проверять распределение элементов ВОР по рабочим наборам.
    /// </summary>
    public bool CheckWorksets { get; set; } = true;

    public ProjectCard Project { get; set; } = new();
}
