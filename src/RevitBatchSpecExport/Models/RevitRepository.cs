using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitBatchSpecExport.Models;

internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new System.ArgumentNullException(nameof(uiApplication));
    }

    public UIApplication UIApplication { get; }

    public Application Application => UIApplication.Application;

    public ICollection<SheetModel> GetSheetModels(Document doc) {
        ICollection<ViewSheet> viewSheets = GetViewSheets(doc);
        var schedules = GetSchedules(doc)
            .GroupBy(s => s.OwnerViewId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        return viewSheets.Select(sheet => new SheetModel(
                sheet,
                schedules.TryGetValue(sheet.Id, out ScheduleModel[] schedulesArr) ? schedulesArr : []))
            .ToArray();
    }

    /// <summary>
    /// Возвращает коллекцию листов из документа
    /// </summary>
    private ICollection<ViewSheet> GetViewSheets(Document doc) {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            .OfType<ViewSheet>()
            .ToArray();
    }

    private ICollection<ScheduleModel> GetSchedules(Document doc) {
        return new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .OfClass(typeof(ScheduleSheetInstance))
            .OfType<ScheduleSheetInstance>()
            .Where(s => CanPlaceScheduleOnSheet(doc.GetElement(s.ScheduleId) as ViewSchedule))
            .Select(s => new ScheduleModel(s))
            .ToArray();
    }

    private bool CanPlaceScheduleOnSheet(ViewSchedule schedule) {
        return schedule is not null
               && !schedule.IsInternalKeynoteSchedule
               && !schedule.IsTitleblockRevisionSchedule
               && !schedule.IsTemplate
               && !schedule.Definition.IsKeySchedule;
    }
}
