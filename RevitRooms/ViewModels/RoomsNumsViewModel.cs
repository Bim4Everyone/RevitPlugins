using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RoomsNumsViewModel : BaseViewModel {
        protected Guid _id;
        private string _prefix;
        private string _suffix;
        private bool _isNumFlats;
        private bool _isNumRooms;
        private bool _isNumRoomsGroup;
        private bool _isNumRoomsSection;
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

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string Suffix {
            get => _suffix;
            set => this.RaiseAndSetIfChanged(ref _suffix, value);
        }

        public bool IsNumFlats {
            get => _isNumFlats;
            set => this.RaiseAndSetIfChanged(ref _isNumFlats, value);
        }

        public bool IsNumRooms {
            get => _isNumRooms;
            set => this.RaiseAndSetIfChanged(ref _isNumRooms, value);
        }

        public bool IsNumRoomsGroup {
            get => _isNumRoomsGroup;
            set => this.RaiseAndSetIfChanged(ref _isNumRoomsGroup, value);
        }

        public bool IsNumRoomsSection {
            get => _isNumRoomsSection;
            set => this.RaiseAndSetIfChanged(ref _isNumRoomsSection, value);
        }

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
