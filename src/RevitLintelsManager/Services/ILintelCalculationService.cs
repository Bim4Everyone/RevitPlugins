using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Единый расчёт по независимым снимкам исходного документа.</summary>
internal interface ILintelCalculationService {
    /// <summary>Готовит тип, положение, параметры и причины неприменимости для одного проёма.</summary>
    /// <param name="opening">Снимок проёма и его геометрической привязки.</param>
    /// <param name="wall">Стена-хост с разрешённым типом.</param>
    /// <param name="above">Факты непрерывной кладки и конструкций над проёмом.</param>
    /// <param name="sides">Измерения боковой кладки и контактов с конструкциями.</param>
    /// <param name="family">Доступность типа, сопоставлений и семантики семейства.</param>
    /// <param name="config">Неизменяемый снимок настроек и правил сессии.</param>
    /// <param name="matchedLintelId">Идентификатор сопоставленной перемычки, если она найдена.</param>
    /// <returns>Эталон для установки, замены и сравнения.</returns>
    LintelCalculation Calculate(ManagerOpeningSnapshot opening, ManagerWallSnapshot wall, AboveOpeningFacts above,
        OpeningBearingFacts sides, LintelFamilyFacts family, ManagerConfigSnapshot config,
        string matchedLintelId = null);

    /// <summary>Сравнивает существующую перемычку с единым расчётом и прочитанными фактами пары.</summary>
    /// <param name="pair">Подтверждённая или смещённая пара с результатом чтения замка.</param>
    /// <param name="calculation">Тот же эталон, который используется при подборе.</param>
    /// <param name="config">Настройки текущей сессии, в том числе целевая стадия создания.</param>
    /// <returns>Отдельные отличия и причины недоступного сравнения.</returns>
    LintelComparison Compare(OpeningLintelMatch pair, LintelCalculation calculation, ManagerConfigSnapshot config);
}
