using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models.ScheduleFilters;

/// <summary>
/// Условие фильтра спецификации с локализованным названием для выпадающего списка
/// </summary>
public class ScheduleTypeInfo {
    public ScheduleTypeInfo(ScheduleFilterType filterType, string name) {
        FilterType = filterType;
        Name = name;
    }

    public ScheduleFilterType FilterType { get; set; }
    public string Name { get; set; }
}
