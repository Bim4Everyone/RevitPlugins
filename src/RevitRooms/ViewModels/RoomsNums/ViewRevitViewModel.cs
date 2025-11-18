using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitRooms.Models;
using RevitRooms.Services;

namespace RevitRooms.ViewModels.RoomsNums;
internal class ViewRevitViewModel : RevitRoomNumsViewModel {
    public ViewRevitViewModel(RevitRepository revitRepository, 
                              RoomsNumsConfig roomsNumsConfig,
                              IMessageBoxService messageBoxService,
                              NumOrderWindowService numOrderWindowService)
        : base(revitRepository, roomsNumsConfig, messageBoxService, numOrderWindowService) {
        _id = new Guid("38DF60C2-1D99-4256-9D41-0CB34A95E0AE");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetRoomsOnActiveView()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
