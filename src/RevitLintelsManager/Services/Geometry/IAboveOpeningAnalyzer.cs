using System.Collections.Generic;

namespace RevitLintelsManager.Models;

/// <summary>Измеряет кладку и конструкции над верхним центром проёма.</summary>
internal interface IAboveOpeningAnalyzer {
    /// <summary>Измеряет высоту непрерывной кладки и расстояние до верхней конструкции.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="openings">Проёмы результирующих строк.</param>
    /// <param name="candidates">Отобранные конструкции выбранной стадии.</param>
    /// <param name="catalogIncomplete">Признак неполного каталога кандидатов.</param>
    /// <param name="minimumMm">Настроенная минимальная высота кладки в миллиметрах.</param>
    /// <returns>Независимые результаты для каждого переданного проёма.</returns>
    IReadOnlyList<AboveOpeningFacts> ReadAboveOpenings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings, IReadOnlyList<StructureCandidate> candidates,
        bool catalogIncomplete, double minimumMm);
}
