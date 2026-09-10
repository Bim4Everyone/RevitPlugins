using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitExportSpecToJson.Comparators;

namespace RevitExportSpecToJson.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
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

    public Transaction StartTransaction(string transactionName) {
        return Document.StartTransaction(transactionName);
    }

    public IEnumerable<Schedule> GetSchedules() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .Cast<ViewSchedule>()
            .Select(viewSchedule => new Schedule(viewSchedule) { Status = GetStatus(viewSchedule) })
            .OrderBy(item => item.Status)
            .ThenBy(item => item.Name, new NamingComparator());
    }

    private ScheduleStatus GetStatus(ViewSchedule viewSchedule) {
        if(viewSchedule.Id == ActiveUIDocument.ActiveView.Id) {
            return ScheduleStatus.Active;
        }

        bool isOpened = ActiveUIDocument.GetOpenUIViews()
            .Any(item => item.ViewId == viewSchedule.Id);

        if(isOpened) {
            return ScheduleStatus.Opened;
        }

        return ScheduleStatus.Closed;
    }
}
