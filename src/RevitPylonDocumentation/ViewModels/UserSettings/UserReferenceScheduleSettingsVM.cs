using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserReferenceScheduleSettingsVM {
    public UserReferenceScheduleSettingsVM(MainViewModel mainViewModel) {
        ViewModel = mainViewModel;
    }

    public MainViewModel ViewModel { get; set; }

    /// <summary>
    /// Эталонная спецификация материалов
    /// </summary>
    public ViewSchedule ReferenceMaterialSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для системной арматуры
    /// </summary>
    public ViewSchedule ReferenceSystemPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для IFC арматуры
    /// </summary>
    public ViewSchedule ReferenceIfcPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonByElemsSchedule { get; set; }


    public UserReferenceScheduleSettings GetSettings() {
        var settings = new UserReferenceScheduleSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserReferenceScheduleSettings);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null && vmProp.CanRead && prop.CanWrite) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }
}
