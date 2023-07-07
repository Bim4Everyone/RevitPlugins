using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit {
    internal class ViewRevitViewModel : RevitViewModel {
        public ViewRevitViewModel(RevitRepository revitRepository)
            : base(revitRepository) {
            _id = new Guid("38DF60C2-1D99-4256-9D41-0CB34A95E0AE");
            foreach(var level in Levels) {
                level.IsSelected = true;
            }
        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            var viewElements = _revitRepository.GetRoomsOnActiveView();
            IEnumerable<SpatialElement> additionalElements = GetAdditionalElements(viewElements);

            return viewElements.Union(additionalElements)
                .Where(item => item.Level != null)
                .GroupBy(item => item.Level.Name.Split('_').FirstOrDefault())
                .Select(item =>
                    new LevelViewModel(item.Key, item.Select(room => room.Level).ToList(), _revitRepository, item));
        }
    }
}