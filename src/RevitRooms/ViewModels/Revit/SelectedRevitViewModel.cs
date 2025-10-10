using System;
using System.Collections.Generic;
using System.Linq;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit;
internal class SelectedRevitViewModel : RevitViewModel {
    public SelectedRevitViewModel(RevitRepository revitRepository, RoomsConfig roomsConfig)
        : base(revitRepository, roomsConfig) {
        _id = new Guid("AAAC541D-16B3-4E82-A702-208B099AB031");
        foreach(var level in Levels) {
            level.IsSelected = true;
        }
    }

    protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
        var selectedElements = _revitRepository.GetSelectedSpatialElements();
        var additionalElements = GetAdditionalElements(selectedElements);

        return selectedElements.Union(additionalElements)
            .Where(item => item.Level != null)
            .GroupBy(item => item.Level.Name.Split('_').FirstOrDefault())
            .Select(item =>
                new LevelViewModel(item.Key, item.Select(room => room.Level).ToList(), _revitRepository, item));
    }
}
