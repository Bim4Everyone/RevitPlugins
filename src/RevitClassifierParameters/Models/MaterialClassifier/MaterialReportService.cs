using System.Collections.Generic;

namespace RevitClassifierParameters.Models.MaterialClassifier;

/// <summary>
/// Сервис сбора отчёта по обработке материалов.
/// </summary>
public class MaterialReportService {
    private readonly List<MaterialReportItem> _items = [];

    /// <summary>
    /// Собранные записи отчёта.
    /// </summary>
    public IReadOnlyList<MaterialReportItem> Items => _items;

    /// <summary>
    /// Добавляет запись в отчёт.
    /// </summary>
    /// <param name="status">Статус обработки материала.</param>
    /// <param name="workCode">Код работы (ключевая заметка материала).</param>
    /// <param name="materialName">Имя материала.</param>
    public void Add(MaterialReportStatus status, string workCode, string materialName) {
        _items.Add(new MaterialReportItem(status, workCode, materialName));
    }

    /// <summary>
    /// Очищает отчёт.
    /// </summary>
    public void Clear() {
        _items.Clear();
    }
}
