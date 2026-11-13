using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitExportSpecToJson.Models;

namespace RevitExportSpecToJson.ViewModels;

internal sealed class ScheduleStatusViewModel : BaseViewModel {
    public ScheduleStatusViewModel(ScheduleStatus status, ILocalizationService localizationService) {
        StatusName = status == ScheduleStatus.Closed
            ? string.Empty
            : localizationService.GetLocalizedString($"{status.GetType().Name}.{status}");
    }

    public string StatusName { get; }

    public override string ToString() => StatusName;
}
