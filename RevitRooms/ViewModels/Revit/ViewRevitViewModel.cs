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
        public ViewRevitViewModel(Application application, Document document)
            : base(application, document) {
        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            return _revitRepository.GetRoomsOnActiveView()
                .GroupBy(item => item.Level, new ElementComparer())
                .Select(item => new LevelViewModel((Level) item.Key, _revitRepository, item));
        }
    }
}