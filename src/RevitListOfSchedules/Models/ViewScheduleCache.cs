using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;
public class ViewScheduleCache {
    private readonly Document _document;
    private readonly Dictionary<string, bool> _viewExistenceCache = new(StringComparer.OrdinalIgnoreCase);

    public ViewScheduleCache(Document document) {
        _document = document;
    }

    public bool IsViewScheduleExist(string name) {
        if(_viewExistenceCache.TryGetValue(name, out bool exists)) {
            return exists;
        }

        exists = new FilteredElementCollector(_document)
            .OfClass(typeof(ViewSchedule))
            .Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        _viewExistenceCache[name] = exists;
        return exists;
    }
}
