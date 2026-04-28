using System.ComponentModel;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.SimpleServices;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels;
internal class SelectedRevitViewModel : RevitViewModel {
    public SelectedRevitViewModel(RevitRepository revitRepository, ILocalizationService localizationService)
        : base(revitRepository, localizationService) {
    }

    protected override BindingList<RoomGroupViewModel> GetGroupViewModels() {
        RevitParam groupParam = ProjectParamsConfig.Instance.RoomGroupName;
        string withoutGroup = _localizationService.GetLocalizedString("MainWindow.RoomWithoutGroup");

        var selectedRoomsList = _revitRepository.GetSelectedRooms()
            .Where(r => r.RoomObject.Area > 0)
            .GroupBy(x => x.RoomObject.GetParamValueStringOrDefault(groupParam, withoutGroup))
            .Select(x => new RoomGroupViewModel(x.Key.ToString(), x, _localizationService))
            .OrderBy(x => x.Name)
            .ToList();

        return [.. selectedRoomsList];
    }
}
