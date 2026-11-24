using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Применяет фильтры семейств и стадии, собирает снимок для анализа менеджером.</summary>
internal interface IProjectSnapshotReader {
    /// <summary>Проверяет наличие подходящих экземпляров по фильтрам полного чтения.</summary>
    /// <param name="collector">Коллектор элементов диапазона, подготовленный вызывающей стороной.</param>
    /// <param name="config">Неизменяемые настройки сессии.</param>
    /// <param name="phaseId">Выбранная стадия исходного документа.</param>
    /// <returns>Наличие хотя бы одного экземпляра.</returns>
    bool HasMatchingFamilies(FilteredElementCollector collector, ManagerConfigSnapshot config, ElementId phaseId);

    /// <summary>Собирает элементы заданного диапазона и факты их геометрического анализа.</summary>
    /// <param name="config">Неизменяемые настройки сессии.</param>
    /// <param name="phaseId">Выбранная стадия исходного документа.</param>
    /// <param name="scope">Диапазон анализа.</param>
    /// <param name="collector">Коллектор элементов диапазона, подготовленный вызывающей стороной.</param>
    /// <returns>Материализованный снимок выбранного диапазона.</returns>
    ManagerProjectSnapshot ReadScopeSnapshot(ManagerConfigSnapshot config, ElementId phaseId,
        ManagerScope scope, FilteredElementCollector collector);
}
