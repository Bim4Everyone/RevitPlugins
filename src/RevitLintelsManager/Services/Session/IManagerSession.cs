using System;
using System.Threading.Tasks;

using Ninject;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

internal interface IManagerSession {
    event Action Released;

    /// <summary>Принимает контейнер и запускает подготовку модели перед показом окна.</summary>
    /// <param name="kernel">Контейнер сессии, без использования для поиска сервисов.</param>
    /// <param name="reportStartupFailure">Показывает ошибку отложенного запуска из UI-потока.</param>
    /// <exception cref="InvalidOperationException">Сессия уже запускалась.</exception>
    void Start(IKernel kernel, Action<Exception> reportStartupFailure);

    /// <summary>Активирует единственное окно и проверяет документ в контексте основной команды.</summary>
    void Activate();

    /// <summary>Подтверждает план из интерфейса и ждёт завершения всего API-пакета.</summary>
    Task<ProcessingBatchResult> ProcessAsync(ProcessingPlan plan);

    /// <summary>Запрашивает закрытие после безопасного завершения pending/active обработчика Revit.</summary>
    void RequestClose();
}
