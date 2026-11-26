using System.Threading;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Установка перемычки внутри транзакции вызывающего действия.</summary>
internal interface ILintelInstallationService {
    /// <summary>Размещает экземпляр по проверенному расчёту и создаёт обязательную зависимость.</summary>
    /// <param name="target">Проём и расчёт текущей операции.</param>
    /// <param name="config">Снимок настроек сессии.</param>
    /// <param name="token">Запрос отмены операции.</param>
    void Install(ProcessingTarget target, ManagerConfigSnapshot config, CancellationToken token);

    /// <summary>Сверяет размещение, атрибуты, расчётные значения и обязательный замок после регенерации.</summary>
    /// <param name="target">Результат размещения в текущей транзакции.</param>
    void Verify(ProcessingTarget target);
}
