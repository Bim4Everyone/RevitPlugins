using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

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

    public IList<ScheduleViewModel> GetSchedulesVM(ILocalizationService localizationService) {
        IList<ViewSchedule> schedulesRevit = GetSchedules();
        IList<ScheduleViewModel> schedules = [];

        ElementId activeViewId = Document.ActiveView.Id;
        IList<ElementId> openedViewIds = ActiveUIDocument
            .GetOpenUIViews()
            .Select(x => x.ViewId)
        .ToList();

        ViewStatuses statuses = new(localizationService);

        foreach(var schedule in schedulesRevit) {
            if(schedule.Id == activeViewId) {
                schedules.Add(new ScheduleViewModel(schedule, statuses.ActiveViewStatus) {
                    IsChecked = true
                });
            } else if(openedViewIds.Contains(schedule.Id)) {
                schedules.Add(new ScheduleViewModel(schedule, statuses.OpenedViewStatus));
            } else {
                schedules.Add(new ScheduleViewModel(schedule, statuses.ClosedViewStatus));
            }
        }

        return schedules;
    }
}
