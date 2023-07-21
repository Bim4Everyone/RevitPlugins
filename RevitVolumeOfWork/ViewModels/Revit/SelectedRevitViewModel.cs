using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels.Revit {
    internal class SelectedRevitViewModel : RevitViewModel {
        public SelectedRevitViewModel(RevitRepository revitRepository) 
            : base(revitRepository) {

        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            return _revitRepository.GetSelectedRooms()
                .Where(item => item.Level != null)
                .GroupBy(item => item.Level)
                .Select(item => new LevelViewModel(item.Key.Name, item.Key, _revitRepository, item));
        }
    }
}
