using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels.Revit {
    internal class ElementsRevitViewModel : RevitViewModel {
        public ElementsRevitViewModel(RevitRepository revitRepository)
            : base(revitRepository) {
            _id = new Guid("19723C2C-75ED-4B0A-8279-8493A949E52F");
            IsAllowSelectLevels = true;
        }

        protected override IEnumerable<LevelViewModel> GetLevelViewModels() {
            return _revitRepository.GetSpatialElements()
                .Where(item => item.Level != null)
                .GroupBy(item => item.Level.Name.Split('_').FirstOrDefault())
                .Select(item =>
                    new LevelViewModel(item.Key, item.Select(room => room.Level).ToList(), _revitRepository, item));
        }
    }
}