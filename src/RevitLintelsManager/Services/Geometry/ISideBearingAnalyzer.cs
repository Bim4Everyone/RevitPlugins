using System.Collections.Generic;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Измеряет кладку и контакты у физических боковых границ проёма.</summary>
internal interface ISideBearingAnalyzer {
    /// <summary>Измеряет опирание и примыкания независимо для каждой стороны.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="openings">Проёмы результирующих строк.</param>
    /// <param name="candidates">Отобранные конструкции выбранной стадии.</param>
    /// <param name="catalogIncomplete">Признак неполного каталога кандидатов.</param>
    /// <param name="config">Неизменяемые настройки сессии.</param>
    /// <returns>Физические стороны проёмов и доказанные признаки крепления.</returns>
    IReadOnlyList<OpeningBearingFacts> ReadSideBearings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings, IReadOnlyList<StructureCandidate> candidates,
        bool catalogIncomplete, ManagerConfigSnapshot config);

    /// <summary>Подтверждает выходящие за торец хоста концы существующих перемычек внутри соседних стен.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="matches">Сопоставленные строки со снимками существующих перемычек.</param>
    /// <param name="walls">Соседние стены выбранной стадии с признаками перечня ЖБ.</param>
    /// <param name="familyId">Идентификатор настроенного семейства перемычек.</param>
    /// <param name="leftSign">Подтверждённое направление левого конца семейства.</param>
    /// <returns>Строки с признаками сохранения фактического конца.</returns>
    IReadOnlyList<OpeningLintelMatch> ReadRetainedEnds(IGeometryReadContext geometry,
        IReadOnlyList<OpeningLintelMatch> matches, IReadOnlyList<StructureCandidate> walls, string familyId, int? leftSign);
}
