using System;

using Ninject;

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

    /// <summary>Запрашивает закрытие после безопасного завершения pending/active обработчика Revit.</summary>
    void RequestClose();
}
