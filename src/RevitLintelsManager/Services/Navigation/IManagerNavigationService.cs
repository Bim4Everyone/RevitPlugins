using System;

using Autodesk.Revit.UI;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

internal enum ManagerNavigationResult { Completed, SourceUnavailable, TargetsUnavailable, BoundsUnavailable }

internal interface IManagerNavigationService {
    /// <summary>Получает или подготавливает персональный вид в API-контексте исходной сессии.</summary>
    /// <param name="app">Приложение из текущего вызова Revit.</param>
    /// <param name="config">Снимок конфигурации, содержащий выбранную стадию.</param>
    /// <returns>UniqueId вида или null, если исходный документ закрыт или неактивен.</returns>
    /// <exception cref="InvalidOperationException">Revit не смог подготовить вид на выбранной стадии.</exception>
    string EnsureView(UIApplication app, ManagerConfigSnapshot config);

    /// <summary>Настраивает и активирует служебный вид для элементов запроса из ExternalEvent.</summary>
    /// <param name="app">Приложение из текущего API-вызова.</param>
    /// <param name="request">Ключи строк и поколение запроса.</param>
    /// <param name="state">Конфигурация и состояние исходной сессии.</param>
    /// <returns>Результат перехода либо ожидаемая причина отказа.</returns>
    /// <exception cref="InvalidOperationException">Revit не смог настроить или показать служебный вид.</exception>
    ManagerNavigationResult Navigate(UIApplication app, ManagerRequest request, ManagerSessionState state);
}
