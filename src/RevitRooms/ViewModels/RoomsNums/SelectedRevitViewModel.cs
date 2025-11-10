using System;
using System.Collections.Generic;
using System.Linq;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.RoomsNums;
internal class SelectedRevitViewModel : RevitViewModel {
    public SelectedRevitViewModel(RevitRepository revitRepository, RoomsNumsConfig roomsNumsConfig)
        : base(revitRepository, roomsNumsConfig) {
        _id = new Guid("AAAC541D-16B3-4E82-A702-208B099AB031");
    }

    protected override IEnumerable<SpatialElementViewModel> GetSpatialElements() {
        return _revitRepository.GetSelectedSpatialElements()
            .Select(item => new SpatialElementViewModel(item, _revitRepository));
    }
}
