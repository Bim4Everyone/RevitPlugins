using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models;
internal class CreationSettings {
    public CreationSettings(UserSelectionSettings selectionSettings,
                            UserVerticalViewSettings verticalViewSettings,
                            UserTransverseViewSettings transverseViewSettings,
                            UserSchedulesSettings schedulesSettings,
                            UserScheduleFiltersSettings scheduleFiltersSettings,
                            UserLegendsAndAnnotationsSettings legendsAndAnnotationsSettings,
                            UserPylonSettings pylonSettings,
                            UserAnnotationSettings annotationSettings,
                            UserDispatcherSettings dispatcherSettings,
                            UserSheetSettings sheetSettings) {
        SelectionSettings = selectionSettings;
        VerticalViewSettings = verticalViewSettings;
        TransverseViewSettings = transverseViewSettings;
        SchedulesSettings = schedulesSettings;
        ScheduleFiltersSettings = scheduleFiltersSettings;
        LegendsAndAnnotationsSettings = legendsAndAnnotationsSettings;
        PylonSettings = pylonSettings;
        AnnotationSettings = annotationSettings;
        DispatcherSettings = dispatcherSettings;
        SheetSettings = sheetSettings;
    }

    public UserSelectionSettings SelectionSettings { get; private set; }
    public UserVerticalViewSettings VerticalViewSettings { get; private set; }
    public UserTransverseViewSettings TransverseViewSettings { get; private set; }
    public UserSchedulesSettings SchedulesSettings { get; private set; }
    public UserScheduleFiltersSettings ScheduleFiltersSettings { get; private set; }
    public UserLegendsAndAnnotationsSettings LegendsAndAnnotationsSettings { get; private set; }
    public UserPylonSettings PylonSettings { get; private set; }
    public UserAnnotationSettings AnnotationSettings { get; private set; }
    public UserDispatcherSettings DispatcherSettings { get; private set; }
    public UserSheetSettings SheetSettings { get; private set; }
}
