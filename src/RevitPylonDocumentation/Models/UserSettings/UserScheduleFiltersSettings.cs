using System.Collections.ObjectModel;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserScheduleFiltersSettings {
    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; } = [];
}
