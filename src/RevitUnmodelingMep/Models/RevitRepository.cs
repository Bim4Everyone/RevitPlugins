using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using Newtonsoft.Json.Linq;

using RevitUnmodelingMep.Models.Entities;

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
    public VisSettingsStorage VisSettingsStorage { get; set; }
    public UnmodelingCreator Creator { get; set; }
    public Document Doc { get; set; }
    public UnmodelingCalculator Calculator { get; set; }

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="u  iApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication, 
        VisSettingsStorage settingsUpdaterWorker, 
        Document document, 
        UnmodelingCreator unmodelingCreator,
        UnmodelingCalculator unmodelingCalculator) {
        UIApplication = uiApplication;
        VisSettingsStorage = settingsUpdaterWorker;
        Creator = unmodelingCreator;
        Calculator = unmodelingCalculator;
        Doc = document;

        VisSettingsStorage.PrepareSettings();
        Creator.StartupChecks();
        

        JObject unmodelingSettings = settingsUpdaterWorker.GetUnmodelingConfig();

    }

    public void CalculateUnmodeling() {
        int lastIndex;
        var configs = UnmodelingConfigReader.LoadUnmodelingConfigs(
            VisSettingsStorage,
            resolveCategoryOption: null,
            out lastIndex);

        using(var t = Doc.StartTransaction("Расчет расходников")) {
            Creator.RemoveUnmodeling();

            if(!Creator.Symbol.IsActive) {
                Creator.Symbol.Activate();
                Doc.Regenerate();
            }

            foreach(var config in configs) {
                List <NewRowElement> newRowElements = Calculator.GetElementsToGenerate(config);
                foreach(var newRowElement in newRowElements) {
                    Creator.CreateNewPosition(newRowElement);
                }
            }

            t.Commit();
        }
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
