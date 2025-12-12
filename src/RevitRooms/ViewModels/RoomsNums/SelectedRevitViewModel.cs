using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitRooms.Models;
using RevitRooms.Services;

namespace RevitRooms.ViewModels.RoomsNums;
internal class SelectedRevitViewModel : RevitRoomNumsViewModel {
    public SelectedRevitViewModel(RevitRepository revitRepository, 
                                  RoomsNumsConfig roomsNumsConfig,
                                  IMessageBoxService messageBoxService,
                                  ILocalizationService localizationService,
                                  NumOrderWindowService numOrderWindowService)
        : base(revitRepository, roomsNumsConfig, messageBoxService, localizationService, numOrderWindowService) {
        _id = new Guid("AAAC541D-16B3-4E82-A702-208B099AB031");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetSelectedSpatialElements()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
