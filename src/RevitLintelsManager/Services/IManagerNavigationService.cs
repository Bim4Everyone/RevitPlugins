using System;

using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

internal interface IManagerNavigationService {
    /// <summary>Получает или подготавливает персональный вид в API-контексте исходной сессии.</summary>
    /// <param name="app">Приложение из текущего вызова Revit.</param>
    /// <param name="config">Снимок конфигурации, содержащий выбранную стадию.</param>
    /// <returns>UniqueId вида или null, если исходный документ закрыт или неактивен.</returns>
    /// <exception cref="InvalidOperationException">Revit не смог подготовить вид на выбранной стадии.</exception>
    string EnsureView(UIApplication app, ManagerConfigSnapshot config);
}
