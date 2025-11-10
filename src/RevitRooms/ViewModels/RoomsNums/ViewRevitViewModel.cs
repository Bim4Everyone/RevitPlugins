using System;
using System.Collections.Generic;
using System.Linq;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.RoomsNums;
internal class ViewRevitViewModel : RevitViewModel {
    public ViewRevitViewModel(RevitRepository revitRepository, RoomsNumsConfig roomsNumsConfig)
        : base(revitRepository, roomsNumsConfig) {
        _id = new Guid("38DF60C2-1D99-4256-9D41-0CB34A95E0AE");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetRoomsOnActiveView()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
