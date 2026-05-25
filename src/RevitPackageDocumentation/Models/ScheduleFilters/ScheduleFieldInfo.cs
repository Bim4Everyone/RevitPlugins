using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models.ScheduleFilters;
internal class ScheduleFieldInfo {
    public ScheduleFieldInfo(ScheduleField field) {
        Field = field;
        FieldName = field.GetName();
    }

    public ScheduleField Field { get; set; }
    public string FieldName { get; set; }
}
