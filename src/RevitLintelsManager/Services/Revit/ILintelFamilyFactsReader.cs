using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Читает сведения о семействе перемычек для расчёта размещения.</summary>
internal interface ILintelFamilyFactsReader {
    /// <summary>Источник актуальных определений на время отслеживания исходного документа.</summary>
    ProjectReadCache SessionCache { set; }

    /// <summary>Читает факты размещения и параметров в API-контексте исходного документа.</summary>
    /// <param name="config">Неизменяемые настройки текущей сессии менеджера.</param>
    /// <param name="sampleId">Уникальный идентификатор образца перемычки или null при его отсутствии.</param>
    /// <returns>Снимок сведений о семействе, доступных типах и активном рабочем наборе.</returns>
    LintelFamilyFacts ReadLintelFamilyFacts(ManagerConfigSnapshot config, string sampleId);
}
