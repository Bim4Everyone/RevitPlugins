using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal abstract class RoomsNumsViewModel : BaseViewModel {
        protected Guid _id;
        private string _errorText;
        private string _prefix;
        private string _suffix;
        private bool _isNumFlats;
        private bool _isNumRooms;
        private bool _isNumRoomsGroup;
        private bool _isNumRoomsSection;
        protected readonly RevitRepository _revitRepository;

        public RoomsNumsViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            var additionalPhases = _revitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, _revitRepository));
            SpatialElements = new ObservableCollection<SpatialElementViewModel>(GetSpartialElements().Where(item => item.Phase != null).Where(item => !additionalPhases.Contains(item.Phase)).Where(item => item.IsPlaced));

            Phases = new ObservableCollection<PhaseViewModel>(GetPhases());
            Levels = new ObservableCollection<IElementViewModel<Level>>(GetLevels());
            Groups = new ObservableCollection<IElementViewModel<Element>>(GetGroups());
            Sections = new ObservableCollection<IElementViewModel<Element>>(GetSections());
            NumberingOrders = new ObservableCollection<NumberingOrderViewModel>(GetNumberingOrders());
            SelectedNumberingOrders = new ObservableCollection<NumberingOrderViewModel>(NumberingOrders.Where(item => item.Order > 0));

            Phase = Phases.FirstOrDefault();
            NumerateRoomsCommand = new RelayCommand(NumerateRooms, CanNumerateRooms);
        }

        public string Name { get; set; }
        protected abstract IEnumerable<SpatialElementViewModel> GetSpartialElements();

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

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

        public ICommand NumerateRoomsCommand { get; }

        public PhaseViewModel Phase { get; set; }
        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<SpatialElementViewModel> SpatialElements { get; }

        public ObservableCollection<IElementViewModel<Level>> Levels { get; }
        public ObservableCollection<IElementViewModel<Element>> Groups { get; }
        public ObservableCollection<IElementViewModel<Element>> Sections { get; }
        public ObservableCollection<NumberingOrderViewModel> NumberingOrders { get; }
        public ObservableCollection<NumberingOrderViewModel> SelectedNumberingOrders { get; }

        private IEnumerable<PhaseViewModel> GetPhases() {
            return SpatialElements.Select(item => item.Phase)
                .Distinct()
                .Except(_revitRepository.GetAdditionalPhases().Select(item => new PhaseViewModel(item, _revitRepository)))
                .OrderBy(item => item.Name);
        }

        private IEnumerable<IElementViewModel<Level>> GetLevels() {
            return SpatialElements
                .Select(item => _revitRepository.GetElement(item.LevelId))
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Level>((Level) item, _revitRepository))
                .Distinct()
                .OrderBy(item => item.Element.Elevation);
        }

        private IEnumerable<IElementViewModel<Element>> GetGroups() {
            return SpatialElements
                .Select(item => item.RoomGroup)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct()
                .OrderBy(item => item.Name);
        }

        private IEnumerable<IElementViewModel<Element>> GetSections() {
            return SpatialElements
                .Select(item => item.RoomSection)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct()
                .OrderBy(item => item.Name);
        }

        private IEnumerable<NumberingOrderViewModel> GetNumberingOrders() {
            return _revitRepository.GetNumberingOrders()
                .Select(item => new NumberingOrderViewModel(item, _revitRepository))
                .OrderBy(item => item.Order);
        }

        private void NumerateRooms(object param) {
            _revitRepository.RemoveUnplacedSpatialElements();

            var levels = Levels.Where(item => item.IsSelected).Select(item => item.ElementId).ToArray();
            var groups = Groups.Where(item => item.IsSelected).Select(item => item.ElementId).ToArray();
            var sections = Sections.Where(item => item.IsSelected).Select(item => item.ElementId).ToArray();

            var workingObjects = SpatialElements
                .Where(item => item.Phase == Phase)
                .Where(item => levels.Contains(item.LevelId))
                .Where(item => item.RoomGroup != null && groups.Contains(item.RoomGroup.Id))
                .Where(item => item.RoomSection != null && sections.Contains(item.RoomSection.Id))
                .OrderBy(item => item.RoomSection.Name)
                .ThenBy(item => (_revitRepository.GetElement(item.LevelId) as Level).Elevation)
                .ThenBy(item => GetDistance(item.Element));

            if(IsNumFlats) {
                using(var transaction = _revitRepository.StartTransaction("Нумерация групп помещений")) {

                    int flatCount = 1;
                    foreach(var level in workingObjects.GroupBy(item => item.LevelId)) {
                        foreach(var flat in level.GroupBy(item => item.RoomGroup.Id)) {
                            foreach(var room in flat) {
                                room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentNumber, Prefix + flatCount + Suffix);
                            }

                            flatCount++;
                        }


                    }

                    transaction.Commit();
                }
            } else {
                var selectedOrder = SelectedNumberingOrders.ToDictionary(item => item.ElementId, item => item.Order);
                workingObjects = workingObjects
                    .ThenBy(item => GetOrder(selectedOrder, item.Room));
                if(IsNumRoomsGroup) {
                    using(var transaction = _revitRepository.StartTransaction("Нумерация помещений по группе")) {
                       
                        foreach(var group in workingObjects.GroupBy(item => item.RoomGroup.Id)) {
                            foreach(var section in group.GroupBy(item => item.RoomSection.Id)) {
                                int roomCount = 1;
                                foreach(var room in section) {
                                    room.Element.SetParamValue(BuiltInParameter.ROOM_NUMBER, Prefix + roomCount + Suffix);
                                    roomCount++;
                                }
                            }

                            
                        }

                        transaction.Commit();
                    }
                } else {
                    using(var transaction = _revitRepository.StartTransaction("Нумерация помещений по секции")) {
                        int roomCount = 1;
                        foreach(var room in workingObjects) {
                            room.Element.SetParamValue(BuiltInParameter.ROOM_NUMBER, Prefix + roomCount + Suffix);
                            roomCount++;
                        }

                        transaction.Commit();
                    }
                }
            }
        }

        private bool CanNumerateRooms(object param) {
            if(Phase == null) {
                ErrorText = "Выберите стадию.";
                return false;
            }

            if(IsNumFlats == false && IsNumRooms == false) {
                ErrorText = "Выберите выберете режим работы.";
                return false;
            }

            if(IsNumRooms && IsNumRoomsGroup == false && IsNumRoomsSection == false) {
                ErrorText = "Выберите выберете режим работы нумерации помещений.";
                return false;
            }

            if(!Sections.Any(item => item.IsSelected)) {
                ErrorText = "Выберите хотя бы одну секцию.";
                return false;
            }

            if(!Groups.Any(item => item.IsSelected)) {
                ErrorText = "Выберите хотя бы одну группу.";
                return false;
            }

            if(!Levels.Any(item => item.IsSelected)) {
                ErrorText = "Выберите хотя бы один уровень.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private double GetDistance(Element element) {
            var point = (element.Location as LocationPoint)?.Point;
            if(point == null) {
                return 0;
            }

            return Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        }

        private int GetOrder(Dictionary<ElementId, int> ordering, Element element) {
            if(ordering.TryGetValue(element.Id, out int value)) {
                return value;
            }

            return 0;
        }
    }
}