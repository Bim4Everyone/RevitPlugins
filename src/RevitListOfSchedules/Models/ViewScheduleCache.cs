using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;

public class ViewScheduleCache {
    private readonly Document _document;
    private readonly Dictionary<string, ViewSchedule> _viewExistenceCache = new(StringComparer.OrdinalIgnoreCase);

    public ViewScheduleCache(Document document) {
        _document = document;
    }

    public ViewSchedule ExistViewSchedule(string name) {
        if(_viewExistenceCache.TryGetValue(name, out var exists)) {
            return exists;
        }
        exists = new FilteredElementCollector(_document)
            .OfClass(typeof(ViewSchedule))
            .Cast<ViewSchedule>()
            .FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        _viewExistenceCache[name] = exists;
        return exists;
    }
}
