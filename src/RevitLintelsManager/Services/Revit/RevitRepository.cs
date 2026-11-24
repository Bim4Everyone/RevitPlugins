using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Приложение и исходный документ, зафиксированный при запуске команды.</summary>
internal sealed class RevitRepository(UIApplication uiApplication) {
    public UIApplication UIApplication { get; } = uiApplication;
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document { get; } = uiApplication.ActiveUIDocument.Document;

    /// <summary>Проверяет идентичность открытого исходного проекта по экземпляру документа Revit.</summary>
    public bool IsSourceDocumentActive(UIApplication app) {
        return Document.IsValidObject && app.ActiveUIDocument?.Document is { IsValidObject: true } activeDocument
            && Document.Equals(activeDocument);
    }

    // Получает стадию из неизменяемой конфигурации текущей сессии.
    public ElementId GetReadPhaseId(ManagerConfigSnapshot config) {
        if(Document.GetElement(config.Phase.UniqueId) is not Phase phase) {
            throw new InvalidOperationException("Настроенная стадия удалена из исходного документа.");
        }
        return phase.Id;
    }
}
