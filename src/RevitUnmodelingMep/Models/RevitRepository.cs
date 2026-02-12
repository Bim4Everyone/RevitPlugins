using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;
using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.Models.Unmodeling;
using RevitUnmodelingMep.ViewModels;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitUnmodelingMep.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
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
        UnmodelingCalculator unmodelingCalculator, 
        ILocalizationService localizationService
        ) {
        _localizationService = localizationService;
        UIApplication = uiApplication;
        VisSettingsStorage = settingsUpdaterWorker;
        Creator = unmodelingCreator;
        Calculator = unmodelingCalculator;
        Doc = document;

        VisSettingsStorage.PrepareSettings();
        Creator.StartupChecks();
        

    }

    public void CalculateUnmodeling() {
        int totalRows;
        var rowsByConfig = PrepareUnmodelingRows(null, CancellationToken.None, out totalRows);
        CreateUnmodelingRows(rowsByConfig, totalRows, null, CancellationToken.None);
    }

    public void CalculateUnmodeling(Func<string, IProgressDialogService> createPercentProgressDialog) {
        if(createPercentProgressDialog == null) {
            CalculateUnmodeling();
            return;
        }

        int totalRows;
        List<List<NewRowElement>> rowsByConfig;

        using(var dialog = createPercentProgressDialog("Repository.PrepareConfigsTitle")) {
            var progress = dialog.CreateProgress();
            var ct = dialog.CreateCancellationToken();
            rowsByConfig = PrepareUnmodelingRows(progress, ct, out totalRows);
        }

        using(var dialog = createPercentProgressDialog("Repository.TransactionName")) {
            var progress = dialog.CreateProgress();
            var ct = dialog.CreateCancellationToken();
            CreateUnmodelingRows(rowsByConfig, totalRows, progress, ct);
        }
    }

    private List<List<NewRowElement>> PrepareUnmodelingRows(
        IProgress<int> progress,
        CancellationToken ct,
        out int totalRows) {

        int lastIndex;
        IReadOnlyList<ConsumableTypeItem> configs = UnmodelingConfigReader.LoadUnmodelingConfigs(
            VisSettingsStorage,
            resolveCategoryOption: null,
            out lastIndex);

        var rowsByConfig = new List<List<NewRowElement>>();
        totalRows = 0;

        var cache = new UnmodelingCalcCache();
        int totalConfigs = configs.Count;
        int preparedConfigs = 0;
        foreach(var config in configs) {
            ct.ThrowIfCancellationRequested();
            PumpUi();
            List<NewRowElement> newRowElements = Calculator.GetElementsToGenerate(config, cache);
            rowsByConfig.Add(newRowElements);
            totalRows += newRowElements.Count;
            preparedConfigs++;

            if(totalConfigs > 0) {
                int percent = (int) System.Math.Floor(preparedConfigs * 100d / totalConfigs);
                progress?.Report(percent);
            }
        }

        return rowsByConfig;
    }

    private void CreateUnmodelingRows(
        IReadOnlyList<List<NewRowElement>> rowsByConfig,
        int totalRows,
        IProgress<int> progress,
        CancellationToken ct) {

        using(var t = Doc.StartTransaction(_localizationService.GetLocalizedString("Repository.TransactionName"))) {
            Creator.RemoveUnmodeling();

            if(!Creator.Symbol.IsActive) {
                Creator.Symbol.Activate();
                Doc.Regenerate();
            }

            int createdCount = 0;
            int lastPercent = 0;
            foreach(var newRowElements in rowsByConfig) {
                foreach(var newRowElement in newRowElements) {
                    ct.ThrowIfCancellationRequested();
                    Creator.CreateNewPosition(newRowElement);

                    if(totalRows > 0) {
                        createdCount++;
                        int percent = (int) System.Math.Floor(createdCount * 100d / totalRows);
                        if(percent > lastPercent) {
                            lastPercent = percent;
                            progress?.Report(percent);
                        }
                    }
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

    private static void PumpUi() {
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        dispatcher.Invoke(() => { }, DispatcherPriority.Background);
    }
}
