using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models;
internal class CreationSettings {
    public CreationSettings(UserSelectionSettings selectionSettings,
                            UserVerticalViewSettings verticalViewSettings,
                            UserHorizontalViewSettings horizontalViewSettings,
                            UserSchedulesSettings schedulesSettings,
                            UserScheduleFiltersSettings scheduleFiltersSettings,
                            UserLegendsAndAnnotationsSettings legendsAndAnnotationsSettings,
                            UserPylonSettings pylonSettings,
                            UserProjectSettings projectSettings,
                            UserSheetSettings sheetSettings) {
        SelectionSettings = selectionSettings;
        VerticalViewSettings = verticalViewSettings;
        HorizontalViewSettings = horizontalViewSettings;
        SchedulesSettings = schedulesSettings;
        ScheduleFiltersSettings = scheduleFiltersSettings;
        LegendsAndAnnotationsSettings = legendsAndAnnotationsSettings;
        PylonSettings = pylonSettings;
        ProjectSettings = projectSettings;
        SheetSettings = sheetSettings;
    }

    public UserSelectionSettings SelectionSettings { get; private set; }
    public UserVerticalViewSettings VerticalViewSettings { get; private set; }
    public UserHorizontalViewSettings HorizontalViewSettings { get; private set; }
    public UserSchedulesSettings SchedulesSettings { get; private set; }
    public UserScheduleFiltersSettings ScheduleFiltersSettings { get; private set; }
    public UserLegendsAndAnnotationsSettings LegendsAndAnnotationsSettings { get; private set; }
    public UserPylonSettings PylonSettings { get; private set; }
    public UserProjectSettings ProjectSettings { get; private set; }
    public UserSheetSettings SheetSettings { get; private set; }
}
