using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
        }

        public string DisplayData { get; set; }
        public ObservableCollection<LevelViewModel> Levels { get; }
        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();
    }
}
