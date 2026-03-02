using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitBatchSpecExport.Models;

public class SheetModel {
    public SheetModel(ViewSheet sheet, ICollection<ScheduleModel> schedules) {
        Sheet = sheet ?? throw new ArgumentNullException(nameof(sheet));
        Schedules = schedules?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(schedules));
    }

    /// <summary>
    /// Лист Revit
    /// </summary>
    public ViewSheet Sheet { get; }

    /// <summary>
    /// Спецификации, размещенные на листе, без дубликатов
    /// </summary>
    public IList<ScheduleModel> Schedules { get; }
}
