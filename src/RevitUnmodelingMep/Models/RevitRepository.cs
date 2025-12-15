using System;
using System.Collections.Generic;
using System.Windows;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using Wpf.Ui.Controls;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitUnmodelingMep.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    public SettingsUpdater SettingsUpdaterWorker { get; set; }
    public Document Doc { get; set; }
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="u  iApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication, SettingsUpdater settingsUpdaterWorker, Document document) {
        UIApplication = uiApplication;
        SettingsUpdaterWorker = settingsUpdaterWorker;
        Doc = document;

        SettingsUpdaterWorker.PrepareSettings();
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }
    
    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    
    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;
    
    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;
}
