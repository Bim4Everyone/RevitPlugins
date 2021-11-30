using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit {
    internal class SelectedRevitViewModel : RevitViewModel {
        public SelectedRevitViewModel(Application application, Document document)
            : base(application, document) {
            _id = new Guid("AAAC541D-16B3-4E82-A702-208B099AB031");
            foreach(var level in Levels) {
                level.IsSelected = true;
            }
        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            var selectedElements = _revitRepository.GetSelectedSpatialElements();
            IEnumerable<SpatialElement> additionalElements = GetAdditionalElements(selectedElements);

            return selectedElements.Union(additionalElements)
                .GroupBy(item => item.Level, new ElementComparer())
                .Select(item => new LevelViewModel((Level) item.Key, _revitRepository, item));
        }
    }
}
