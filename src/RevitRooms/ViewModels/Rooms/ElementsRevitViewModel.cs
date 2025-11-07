using System;
using System.Collections.Generic;
using System.Linq;

using RevitRooms.Models;
using RevitRooms.Services;

namespace RevitRooms.ViewModels.Rooms;
internal class ElementsRevitViewModel : RevitViewModel {
    public ElementsRevitViewModel(RevitRepository revitRepository, RoomsConfig roomsConfig, ErrorWindowService errorWindowService)
        : base(revitRepository, roomsConfig, errorWindowService) {
        _id = new Guid("19723C2C-75ED-4B0A-8279-8493A949E52F");
        IsAllowSelectLevels = true;
    }

    protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
        return _revitRepository.GetSpatialElements()
            .Where(item => item.Level != null)
            .GroupBy(item => item.Level.Name.Split('_').FirstOrDefault())
            .Select(item =>
                new LevelViewModel(item.Key, item.Select(room => room.Level).ToList(), _revitRepository, item));
    }
}
