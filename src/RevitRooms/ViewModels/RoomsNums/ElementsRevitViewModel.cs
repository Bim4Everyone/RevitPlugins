using System;
using System.Collections.Generic;
using System.Linq;

using RevitRooms.Models;
using RevitRooms.Services;

namespace RevitRooms.ViewModels.RoomsNums;
internal class ElementsRevitViewModel : RevitViewModel {
    public ElementsRevitViewModel(RevitRepository revitRepository, 
                                  RoomsNumsConfig roomsNumsConfig, 
                                  NumOrderWindowService numOrderWindowService)
        : base(revitRepository, roomsNumsConfig, numOrderWindowService) {
        _id = new Guid("19723C2C-75ED-4B0A-8279-8493A949E52F");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetSpatialElements()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
