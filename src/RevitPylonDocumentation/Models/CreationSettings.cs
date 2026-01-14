using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models;
internal class CreationSettings {
    public CreationSettings(UserProjectSettings projectSettings,
                            UserSheetSettings sheetSettings,
                            UserPylonSettings pylonSettings,
                            UserSchedulesSettings schedulesSettings,
                            UserScheduleFiltersSettings scheduleFiltersSettings,
                            UserSelectionSettings selectionSettings,
                            UserVerticalViewSettings verticalViewSettings,
                            UserHorizontalViewSettings horizontalViewSettings,
                            UserLegendsAndAnnotationsSettings legendsAndAnnotationsSettings,
                            UserReferenceScheduleSettings referenceScheduleSettings) {
        ProjectSettings = projectSettings;
        SheetSettings = sheetSettings;
        PylonSettings = pylonSettings;
        SchedulesSettings = schedulesSettings;
        ScheduleFiltersSettings = scheduleFiltersSettings;
        SelectionSettings = selectionSettings;
        VerticalViewSettings = verticalViewSettings;
        HorizontalViewSettings = horizontalViewSettings;
        LegendsAndAnnotationsSettings = legendsAndAnnotationsSettings;
        ReferenceScheduleSettings = referenceScheduleSettings;
    }

    public UserProjectSettings ProjectSettings { get; private set; }
    public UserSheetSettings SheetSettings { get; private set; }
    public UserPylonSettings PylonSettings { get; private set; }
    public UserSchedulesSettings SchedulesSettings { get; private set; }
    public UserScheduleFiltersSettings ScheduleFiltersSettings { get; private set; }
    public UserSelectionSettings SelectionSettings { get; private set; }
    public UserVerticalViewSettings VerticalViewSettings { get; private set; }
    public UserHorizontalViewSettings HorizontalViewSettings { get; private set; }
    public UserLegendsAndAnnotationsSettings LegendsAndAnnotationsSettings { get; private set; }
    public UserReferenceScheduleSettings ReferenceScheduleSettings { get; private set; }
}
