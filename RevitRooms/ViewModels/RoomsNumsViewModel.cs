using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RoomsNumsViewModel {
        protected Guid _id;
        protected readonly RevitRepository _revitRepository;

        public RoomsNumsViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);
            SpatialElements = new ObservableCollection<SpatialElementViewModel>(GetSpartialElements().Where(item => item.Phase != null));

            Phases = new ObservableCollection<PhaseViewModel>(GetPhases());
            Levels = new ObservableCollection<IElementViewModel<Level>>(GetLevels());
            Groups = new ObservableCollection<IElementViewModel<Element>>(GetGroups());
            Sections = new ObservableCollection<IElementViewModel<Element>>(GetSections());

            Phase = Phases.FirstOrDefault();
        }

        public string Name { get; set; }
        protected abstract IEnumerable<SpatialElementViewModel> GetSpartialElements();

        public PhaseViewModel Phase { get; set; }
        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<SpatialElementViewModel> SpatialElements { get; }

        public ObservableCollection<IElementViewModel<Level>> Levels { get; }
        public ObservableCollection<IElementViewModel<Element>> Groups { get; }
        public ObservableCollection<IElementViewModel<Element>> Sections { get; }

        private IEnumerable<PhaseViewModel> GetPhases() {
            return SpatialElements.Select(item => item.Phase)
                .Distinct()
                .Except(_revitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, _revitRepository)));
        }

        private IEnumerable<IElementViewModel<Level>> GetLevels() {
            return SpatialElements
                .Select(item => _revitRepository.GetElement(item.LevelId))
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Level>((Level) item, _revitRepository))
                .Distinct();
        }

        private IEnumerable<IElementViewModel<Element>> GetGroups() {
            return SpatialElements
                .Select(item => item.RoomGroup)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct();
        }

        private IEnumerable<IElementViewModel<Element>> GetSections() {
            return SpatialElements
                .Select(item => item.RoomSection)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct();
        }
    }
}
