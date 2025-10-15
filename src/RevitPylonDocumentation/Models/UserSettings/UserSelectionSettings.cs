namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserSelectionSettings {
    public bool NeedWorkWithGeneralView { get; set; }
    public bool NeedWorkWithGeneralPerpendicularView { get; set; }
    public bool NeedWorkWithTransverseViewFirst { get; set; }
    public bool NeedWorkWithTransverseViewSecond { get; set; }
    public bool NeedWorkWithTransverseViewThird { get; set; }
    public bool NeedWorkWithMaterialSchedule { get; set; }
    public bool NeedWorkWithSystemPartsSchedule { get; set; }
    public bool NeedWorkWithIfcPartsSchedule { get; set; }
    public bool NeedWorkWithLegend { get; set; }
    public bool NeedWorkWithGeneralRebarView { get; set; }
    public bool NeedWorkWithGeneralPerpendicularRebarView { get; set; }
    public bool NeedWorkWithSkeletonSchedule { get; set; }
    public bool NeedWorkWithSkeletonByElemsSchedule { get; set; }
    public string SelectedProjectSection { get; set; }
}
