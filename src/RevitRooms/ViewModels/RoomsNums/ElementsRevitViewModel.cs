using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitRooms.Models;
using RevitRooms.Services;

namespace RevitRooms.ViewModels.RoomsNums;
internal class ElementsRevitViewModel : RevitRoomNumsViewModel {
    public ElementsRevitViewModel(RevitRepository revitRepository, 
                                  RoomsNumsConfig roomsNumsConfig,
                                  IMessageBoxService messageBoxService,
                                  NumOrderWindowService numOrderWindowService)
        : base(revitRepository, roomsNumsConfig, messageBoxService, numOrderWindowService) {
        _id = new Guid("19723C2C-75ED-4B0A-8279-8493A949E52F");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetSpatialElements()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
