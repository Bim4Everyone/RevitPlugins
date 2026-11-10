using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models.ScheduleFilters;

/// <summary>
/// Поле спецификации с его именем для выпадающего списка полей фильтра
/// </summary>
internal class ScheduleFieldInfo {
    public ScheduleFieldInfo(ScheduleField field) {
        Field = field;
        FieldName = field.GetName();
    }

    public ScheduleField Field { get; set; }
    public string FieldName { get; set; }
}
