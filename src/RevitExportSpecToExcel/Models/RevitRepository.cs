using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitExportSpecToExcel.ViewModels;

namespace RevitExportSpecToExcel.Models;

internal class RevitRepository {
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;

    public IList<ViewSchedule> GetSchedules() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .OfType<ViewSchedule>()
            .ToList();
    }

    public IList<ScheduleViewModel> GetSchedulesVM() {
        IList<ViewSchedule> schedulesRevit = GetSchedules();
        List<ScheduleViewModel> schedules = [];

        ElementId activeViewId = Document.ActiveView.Id;
        IList<ElementId> openedViewIds = ActiveUIDocument
            .GetOpenUIViews()
            .Select(x => x.ViewId)
            .ToList();

        foreach(var schedule in schedulesRevit) {
            if(schedule.Id == activeViewId) {
                schedules.Add(new ScheduleViewModel(schedule, OpenStatus.ActiveView));
            } else if(openedViewIds.Contains(schedule.Id)) {
                schedules.Add(new ScheduleViewModel(schedule, OpenStatus.OpenedView));
            } else {
                schedules.Add(new ScheduleViewModel(schedule, OpenStatus.OtherVIew));
            }
        }

        return schedules;
    }
}
