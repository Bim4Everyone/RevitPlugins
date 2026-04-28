using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels;
internal class RoomGroupViewModel : BaseViewModel {
    private bool _isChecked;

    public RoomGroupViewModel(string name, IEnumerable<RoomFromRevit> rooms, ILocalizationService localizationService) {
        Name = name;
        Rooms = rooms.ToList();

        RevitParam sectionParam = SharedParamsConfig.Instance.RoomSectionShortName;
        string withoutSection = localizationService.GetLocalizedString("MainWindow.RoomWithoutSection");
        Apartments = Rooms
            .GroupBy(r => r.RoomObject.GetParamValueOrDefault(sectionParam, withoutSection))
            .Select(x => new Apartment(x))
            .ToList();
    }

    public string Name { get; }
    public IReadOnlyCollection<RoomFromRevit> Rooms { get; }
    public IReadOnlyCollection<Apartment> Apartments { get; }

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
}
