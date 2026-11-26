using System;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Диагностика, группировка и политика действий по фактам анализа.</summary>
internal interface IManagerAnalysisService {
    /// <summary>Формирует диагностированную строку для конкретного сопоставления.</summary>
    /// <param name="documentToken">Идентификатор исходного документа в открытой сессии.</param>
    /// <param name="match">Пара или одиночный элемент после поиска в документе.</param>
    /// <param name="calculation">Единый расчёт проёма при наличии проёма.</param>
    /// <param name="config">Неизменяемые настройки сессии.</param>
    /// <returns>Данные строки с ключом, предупреждениями и допустимыми действиями.</returns>
    ManagerAnalyzedRow Analyze(Guid documentToken, OpeningLintelMatch match, LintelCalculation calculation,
        ManagerConfigSnapshot config);

    /// <summary>Возвращает также сравнение, определившее доступные исправления проверенной пары.</summary>
    /// <param name="documentToken">Идентификатор исходного документа сессии.</param>
    /// <param name="match">Актуальное сопоставление в исходном документе.</param>
    /// <param name="calculation">Единый расчёт проёма, если он присутствует.</param>
    /// <param name="config">Настройки открытой сессии.</param>
    /// <param name="comparison">Отличия пары, которые определили состав исправления.</param>
    /// <returns>Та же проанализированная строка, которую получает менеджер.</returns>
    ManagerAnalyzedRow AnalyzeForProcessing(Guid documentToken, OpeningLintelMatch match, LintelCalculation calculation,
        ManagerConfigSnapshot config, out LintelComparison comparison);
}
