using System;

using Ninject;

namespace RevitLintelsManager.Services;

internal interface IManagerSession {
    event Action Released;

    /// <summary>Открывает окно и принимает владение контейнером после успешного запуска.</summary>
    /// <param name="kernel">Контейнер сессии, без использования для поиска сервисов.</param>
    /// <exception cref="InvalidOperationException">Сессия уже запускалась.</exception>
    void Start(IKernel kernel);

    /// <summary>Активирует единственное окно и проверяет документ в контексте основной команды.</summary>
    void Activate();

    /// <summary>Запрашивает закрытие после безопасного завершения pending/active обработчика Revit.</summary>
    void RequestClose();
}
