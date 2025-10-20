using System.Collections.ObjectModel;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserSchedulesSettings {
    public string MaterialSchedulePrefix { get; set; }
    public string MaterialScheduleSuffix { get; set; }
    public string SystemPartsSchedulePrefix { get; set; }
    public string SystemPartsScheduleSuffix { get; set; }
    public string IfcPartsSchedulePrefix { get; set; }
    public string IfcPartsScheduleSuffix { get; set; }
    public string SkeletonSchedulePrefix { get; set; }
    public string SkeletonScheduleSuffix { get; set; }
    public string SkeletonByElemsSchedulePrefix { get; set; }
    public string SkeletonByElemsScheduleSuffix { get; set; }
    public string SkeletonScheduleName { get; set; }
    public string SkeletonByElemsScheduleName { get; set; }
    public string MaterialScheduleName { get; set; }
    public string SystemPartsScheduleName { get; set; }
    public string IfcPartsScheduleName { get; set; }
    public string MaterialScheduleDisp1 { get; set; }
    public string SystemPartsScheduleDisp1 { get; set; }
    public string IfcPartsScheduleDisp1 { get; set; }
    public string SkeletonScheduleDisp1 { get; set; }
    public string SkeletonByElemsScheduleDisp1 { get; set; }
    public string MaterialScheduleDisp2 { get; set; }
    public string SystemPartsScheduleDisp2 { get; set; }
    public string IfcPartsScheduleDisp2 { get; set; }
    public string SkeletonScheduleDisp2 { get; set; }
    public string SkeletonByElemsScheduleDisp2 { get; set; }
    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; } = [];
}
