using System.Threading;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Исправление существующей перемычки и проверка постусловий операции.</summary>
internal interface ILintelCorrectionService {
    /// <summary>Исправляет выявленные расхождения существующего экземпляра в транзакции строки.</summary>
    /// <param name="target">Повторно проверенная пара и её расчёт.</param>
    /// <param name="config">Настройки текущей сессии.</param>
    /// <param name="token">Отмена операции до фиксации.</param>
    void Apply(ProcessingTarget target, ManagerConfigSnapshot config, CancellationToken token);

    /// <summary>Повторяет сравнение и проверяет сохранённые данные и зависимости после регенерации.</summary>
    /// <param name="target">Исправленный экземпляр и снимок исходного состояния.</param>
    /// <param name="config">Настройки текущей сессии.</param>
    void Verify(ProcessingTarget target, ManagerConfigSnapshot config);
}
