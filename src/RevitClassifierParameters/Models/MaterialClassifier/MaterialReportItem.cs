namespace RevitClassifierParameters.Models.MaterialClassifier;

/// <summary>
/// Статус обработки материала.
/// </summary>
public enum MaterialReportStatus {
    /// <summary>
    /// Параметры материала были изменены.
    /// </summary>
    Edited,

    /// <summary>
    /// Параметры материала не изменялись (значения уже актуальны).
    /// </summary>
    NotEdited,

    /// <summary>
    /// У материала не указан код работы (ключевая заметка).
    /// </summary>
    NoWorkCode,

    /// <summary>
    /// Код работы материала не найден в Классификаторе.
    /// </summary>
    ClassifierCodeNotFound,

    /// <summary>
    /// При записи параметров произошла ошибка.
    /// </summary>
    Error
}

/// <summary>
/// Запись отчёта по обработке одного материала.
/// Аналог строки отчёта из Python-плагина: [Статус, Код работы, Имя материала].
/// </summary>
public class MaterialReportItem {
    /// <summary>
    /// Создает запись отчёта.
    /// </summary>
    /// <param name="status">Статус обработки материала.</param>
    /// <param name="workCode">Код работы (ключевая заметка материала).</param>
    /// <param name="materialName">Имя материала.</param>
    public MaterialReportItem(MaterialReportStatus status, string workCode, string materialName) {
        Status = status;
        WorkCode = workCode;
        MaterialName = materialName;
    }

    /// <summary>
    /// Статус обработки материала.
    /// </summary>
    public MaterialReportStatus Status { get; }

    /// <summary>
    /// Код работы (ключевая заметка материала).
    /// </summary>
    public string WorkCode { get; }

    /// <summary>
    /// Имя материала.
    /// </summary>
    public string MaterialName { get; }
}
