using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Диапазон немодальной сессии и результат его чтения в исходном документе.</summary>
internal interface IManagerScopeReader {
    /// <summary>Определяет стартовый диапазон и фиксирует вид и выделение.</summary>
    /// <param name="config">Проверенная конфигурация запуска.</param>
    /// <returns>Состояние сессии без объектов Revit.</returns>
    /// <exception cref="System.InvalidOperationException">Выбранная стадия удалена из документа.</exception>
    ManagerSessionState CaptureInitialState(ManagerConfigSnapshot config);

    /// <summary>Фиксирует признаки вида или выделения, влияющие на диапазон.</summary>
    /// <param name="scope">Текущий диапазон менеджера.</param>
    /// <returns>Данные для сравнения при следующем Idling.</returns>
    ManagerScopeStamp CaptureScopeStamp(ManagerScope scope);

    /// <summary>Читает факты исходного документа в Execute обработчика ExternalEvent.</summary>
    /// <param name="app">Приложение текущего вызова.</param>
    /// <param name="request">Диапазон, токен и поколение запроса.</param>
    /// <param name="state">Исходные настройки и контекст сессии.</param>
    /// <returns>Снимок или null при недоступном исходном документе.</returns>
    /// <exception cref="System.InvalidOperationException">Выбранная стадия удалена из документа.</exception>
    ManagerDocumentContext ReadDocumentContext(UIApplication app, ManagerRequest request, ManagerSessionState state);
}
