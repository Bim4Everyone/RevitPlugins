using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models;
internal class CreationSettings {
    public CreationSettings(UserProjectSettings projectSettings, UserSchedulesSettings schedulesSettings,
                            UserSelectionSettings selectionSettings, UserVerticalViewSettings verticalViewSettings,
                            UserHorizontalViewSettings horizontalViewSettings, UserTypesSettings typesSettings,
                            UserReferenceScheduleSettings referenceScheduleSettings) {
        ProjectSettings = projectSettings;
        SchedulesSettings = schedulesSettings;
        SelectionSettings = selectionSettings;
        VerticalViewSettings = verticalViewSettings;
        HorizontalViewSettings = horizontalViewSettings;
        TypesSettings = typesSettings;
        ReferenceScheduleSettings = referenceScheduleSettings;
    }

    public UserProjectSettings ProjectSettings { get; private set; }
    public UserSchedulesSettings SchedulesSettings { get; private set; }
    public UserSelectionSettings SelectionSettings { get; private set; }
    public UserVerticalViewSettings VerticalViewSettings { get; private set; }
    public UserHorizontalViewSettings HorizontalViewSettings { get; private set; }
    public UserTypesSettings TypesSettings { get; private set; }
    public UserReferenceScheduleSettings ReferenceScheduleSettings { get; private set; }
}
