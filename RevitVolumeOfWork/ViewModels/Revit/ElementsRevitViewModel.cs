using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels.Revit {
    internal class ElementsRevitViewModel : RevitViewModel {
        public ElementsRevitViewModel(RevitRepository revitRepository)
            : base(revitRepository) {

        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            return _revitRepository.GetRooms()
                .Where(item => item.Level != null)
                .GroupBy(item => item.Level.Name)
                .Select(item => new LevelViewModel(item.Key, item.Select(room => room.Level).FirstOrDefault(), _revitRepository, item));
        }
    }
}
