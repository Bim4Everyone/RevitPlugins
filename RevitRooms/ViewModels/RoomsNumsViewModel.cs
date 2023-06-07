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
using RevitRooms.Commands;
using RevitRooms.Views;

using dosymep.Bim4Everyone.ProjectParams;

using Autodesk.Revit.UI;

using DevExpress.Mvvm.Native;

namespace RevitRooms.ViewModels {
    internal abstract class RoomsNumsViewModel : BaseViewModel, INumberingOrder {
        public Guid _id;
        private string _errorText;
        private string _prefix;
        private string _suffix;
        private bool _isNumFlats;
        private bool _isNumRooms;
        private bool _isNumRoomsGroup;
        private bool _isNumRoomsSection;
        private bool _isNumRoomsSectionLevels;
        private string _startNumber;
        protected readonly RevitRepository _revitRepository;

        public System.Windows.Window ParentWindow { get; set; }

        public RoomsNumsViewModel(Application application, Document document) {
            _revitRepository = new RevitRepository(application, document);

            var additionalPhases = _revitRepository.GetAdditionalPhases()
                .Select(item => new PhaseViewModel(item, _revitRepository));
            SpatialElements = new ObservableCollection<SpatialElementViewModel>(GetSpartialElements()
                .Where(item => item.Phase != null).Where(item => !additionalPhases.Contains(item.Phase))
                .Where(item => item.IsPlaced));

            Phases = new ObservableCollection<PhaseViewModel>(GetPhases());
            Levels = new ObservableCollection<LevelViewModel>(GetLevels());
            Groups = new ObservableCollection<IElementViewModel<Element>>(GetGroups());
            Sections = new ObservableCollection<IElementViewModel<Element>>(GetSections());
            NumberingOrders =
                new ObservableCollection<NumberingOrderViewModel>(GetNumberingOrders().Where(item => item.Order == 0));
            SelectedNumberingOrders =
                new ObservableCollection<NumberingOrderViewModel>(GetNumberingOrders().Where(item => item.Order > 0));

            Phase = Phases.FirstOrDefault();
            NumerateRoomsCommand = new RelayCommand(NumerateRooms, CanNumerateRooms);

            UpOrderCommand = new UpOrderCommand(this);
            DownOrderCommand = new DownOrderCommand(this);
            AddOrderCommand = new AddOrderCommand(this);
            RemoveOrderCommand = new RemoveOrderCommand(this);
            SaveOrderCommand = new SaveOrderCommand(this, _revitRepository);

            StartNumber = "1";
            SetRoomsNumsConfig();
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

        public string StartNumber {
            get => _startNumber;
            set => this.RaiseAndSetIfChanged(ref _startNumber, value);
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

        public bool IsNumRoomsSectionLevels {
            get => _isNumRoomsSectionLevels;
            set => this.RaiseAndSetIfChanged(ref _isNumRoomsSectionLevels, value);
        }

        public ICommand NumerateRoomsCommand { get; }

        public ICommand UpOrderCommand { get; }
        public ICommand DownOrderCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand RemoveOrderCommand { get; }
        public ICommand SaveOrderCommand { get; }

        public PhaseViewModel Phase { get; set; }
        public ObservableCollection<PhaseViewModel> Phases { get; }
        public ObservableCollection<SpatialElementViewModel> SpatialElements { get; }

        public ObservableCollection<LevelViewModel> Levels { get; }
        public ObservableCollection<IElementViewModel<Element>> Groups { get; }
        public ObservableCollection<IElementViewModel<Element>> Sections { get; }
        public ObservableCollection<NumberingOrderViewModel> NumberingOrders { get; }
        public ObservableCollection<NumberingOrderViewModel> SelectedNumberingOrders { get; }

        private IEnumerable<PhaseViewModel> GetPhases() {
            return SpatialElements.Select(item => item.Phase)
                .Distinct()
                .Except(_revitRepository.GetAdditionalPhases()
                    .Select(item => new PhaseViewModel(item, _revitRepository)))
                .OrderBy(item => item.Name);
        }

        private IEnumerable<LevelViewModel> GetLevels() {
            return SpatialElements
                .Select(item => _revitRepository.GetElement(item.LevelId))
                .Where(item => item != null)
                .OfType<Level>()
                .GroupBy(item => item.Name.Split('_').FirstOrDefault())
                .Select(item =>
                    new LevelViewModel(item.Key, item.ToList(), _revitRepository,
                        SpatialElements.Select(room => room.Element)))
                .Distinct()
                .OrderBy(item => item.Element.Elevation);
        }

        private IEnumerable<IElementViewModel<Element>> GetGroups() {
            return SpatialElements
                .Select(item => item.RoomGroup)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct()
                .OrderBy(item => item.Element, new dosymep.Revit.Comparators.ElementComparer());
        }

        private IEnumerable<IElementViewModel<Element>> GetSections() {
            return SpatialElements
                .Select(item => item.RoomSection)
                .Where(item => item != null)
                .Select(item => new ElementViewModel<Element>(item, _revitRepository))
                .Distinct()
                .OrderBy(item => item.Element, new dosymep.Revit.Comparators.ElementComparer());
        }

        private IEnumerable<NumberingOrderViewModel> GetNumberingOrders() {
            return _revitRepository.GetNumberingOrders()
                .Select(item => new NumberingOrderViewModel(item, _revitRepository))
                .OrderBy(item => item.Order);
        }

        private void NumerateRooms(object param) {
            _revitRepository.RemoveUnplacedSpatialElements();

            var startNumber = GetStartNumber();

            var levels = Levels
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Levels
                    .Select(level => level.Id))
                .ToArray();

            var groups = Groups
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId)
                .ToArray();

            var sections = Sections
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId)
                .ToArray();

            var workingObjects = SpatialElements
                .Where(item => item.Phase == Phase)
                .Where(item => levels.Contains(item.LevelId))
                .ToArray();

            if(CheckWorkingObjects(workingObjects)) {
                return;
            }

            var orderedObjects = workingObjects
                .Where(item => item.RoomGroup != null && groups.Contains(item.RoomGroup.Id))
                .Where(item => item.RoomSection != null && sections.Contains(item.RoomSection.Id))
                .ToArray();

            var notFoundNames = GetNotFoundNames(orderedObjects);
            if(notFoundNames.Length > 0) {
                ShowNotFoundNames(notFoundNames);
                return;
            }

            if(IsNumFlats) {
                using(var transaction = _revitRepository.StartTransaction("Нумерация групп помещений")) {
                    orderedObjects = orderedObjects
                        .OrderBy(item => item.RoomGroup, new dosymep.Revit.Comparators.ElementComparer())
                        .ThenBy(item => item.RoomSection, new dosymep.Revit.Comparators.ElementComparer())
                        .ThenBy(item => (_revitRepository.GetElement(item.LevelId) as Level)?.Elevation)
                        .ThenBy(item => GetDistance(item.Element))
                        .ThenByDescending(item => item.IsRoomMainLevel)
                        .ToArray();

                    int flatCount = startNumber;
                    foreach(var section in orderedObjects.GroupBy(item => item.RoomSection.Id)) {
                        foreach(var level in section.GroupBy(item => item.LevelId)) {
                            foreach(var group in level.GroupBy(
                                        item => new {item.RoomGroup.Id, item.RoomMultilevelGroup})) {
                                
                                foreach(var room in group) {
                                    room.Element.SetParamValue(SharedParamsConfig.Instance.ApartmentNumber,
                                        Prefix + flatCount + Suffix);
                                }

                                flatCount++;
                            }
                        }
                    }

                    transaction.Commit();
                }
            } else {
                UpdateNumeringOrder();

                var selectedOrder = SelectedNumberingOrders.ToDictionary(item => item.ElementId, item => item.Order);

                if(IsNumRoomsGroup) {
                    using(var transaction = _revitRepository.StartTransaction("Нумерация помещений по группе")) {
                        orderedObjects = orderedObjects
                            .OrderBy(item => item.RoomSection, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => item.RoomGroup, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => (_revitRepository.GetElement(item.LevelId) as Level)?.Elevation)
                            .ThenBy(item => GetOrder(selectedOrder, item.Room))
                            .ThenBy(item => GetDistance(item.Element))
                            .ToArray();

                        foreach(var section in orderedObjects.GroupBy(item => item.RoomSection.Id)) {
                            foreach(var level in section.GroupBy(item => item.LevelId)) {
                                foreach(var group in level.GroupBy(item => item.RoomGroup.Id)) {
                                    int roomCount = startNumber;
                                    foreach(var room in group) {
                                        room.Element.SetParamValue(BuiltInParameter.ROOM_NUMBER,
                                            Prefix + roomCount + Suffix);
                                        roomCount++;
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                    }
                } else if(IsNumRoomsSection) {
                    using(var transaction = _revitRepository.StartTransaction("Нумерация помещений по секции")) {
                        orderedObjects = orderedObjects
                            .OrderBy(item => item.RoomSection, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => (_revitRepository.GetElement(item.LevelId) as Level)?.Elevation)
                            .ThenBy(item => item.RoomGroup, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => item.RoomMultilevelGroup)
                            .ThenBy(item => GetOrder(selectedOrder, item.Room))
                            .ThenBy(item => GetDistance(item.Element))
                            .ToArray();

                        int roomCount = startNumber;
                        foreach(var room in orderedObjects) {
                            room.Element.SetParamValue(BuiltInParameter.ROOM_NUMBER, Prefix + roomCount + Suffix);
                            roomCount++;
                        }

                        transaction.Commit();
                    }
                } else if(IsNumRoomsSectionLevels) {
                    using(var transaction =
                          _revitRepository.StartTransaction("Нумерация помещений по секции и этажу")) {
                        orderedObjects = orderedObjects
                            .OrderBy(item => item.RoomSection, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => (_revitRepository.GetElement(item.LevelId) as Level)?.Elevation)
                            .ThenBy(item => item.RoomGroup, new dosymep.Revit.Comparators.ElementComparer())
                            .ThenBy(item => GetOrder(selectedOrder, item.Room))
                            .ThenBy(item => GetDistance(item.Element))
                            .ToArray();

                        foreach(var section in orderedObjects.GroupBy(item => item.RoomSection.Id)) {
                            foreach(var level in section.GroupBy(item =>
                                        (_revitRepository.GetElement(item.LevelId) as Level)?.Name.Split('_')
                                        .FirstOrDefault())) {
                                int roomCount = startNumber;
                                foreach(var group in level
                                            .OrderBy(item => item.RoomGroup,
                                                new dosymep.Revit.Comparators.ElementComparer())
                                            .GroupBy(item => item.RoomGroup.Id)) {
                                    foreach(var room in group) {
                                        room.Element.SetParamValue(BuiltInParameter.ROOM_NUMBER,
                                            Prefix + roomCount + Suffix);
                                        roomCount++;
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                    }
                } else {
                    throw new InvalidOperationException("Выбран неизвестный режим работы.");
                }
            }

            SaveRoomsNumsConfig();

            ParentWindow.DialogResult = true;
            ParentWindow.Close();
            TaskDialog.Show("Предупреждение!", "Расчет завершен!");
        }

        private bool ShowNotFoundNames(string[] notFoundNames) {
            var taskDialog = new TaskDialog("Квартирография Стадии П.") {
                AllowCancellation = true,
                MainInstruction = "В списке приоритетов отсутствуют наименования помещений.",
                MainContent = " - " + string.Join(Environment.NewLine + " - ", notFoundNames)
            };

            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Добавить отсутствующие?");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Выход");
            if(taskDialog.Show() == TaskDialogResult.CommandLink1) {
                var selection = NumberingOrders
                    .Where(item => notFoundNames.Contains(item.Name))
                    .ToList();

                SelectNumberingOrder(selection);
            }

            return notFoundNames.Length > 0;
        }

        public void SelectNumberingOrder(IEnumerable<NumberingOrderViewModel> selection) {
            foreach(var selected in selection) {
                NumberingOrders.Remove(selected);
                SelectedNumberingOrders.Add(selected);
            }
        }

        private string[] GetNotFoundNames(IEnumerable<SpatialElementViewModel> orderedObjects) {
            if(IsNumFlats) {
                return new string[0];
            }

            return orderedObjects
                .Select(item => item.Room.Name)
                .Except(SelectedNumberingOrders.Select(item => item.Name))
                .Distinct()
                .OrderBy(item => item)
                .ToArray();
        }

        private void UpdateNumeringOrder() {
            int count = 0;
            foreach(var order in SelectedNumberingOrders) {
                order.Order = ++count;
            }
        }

        private bool CheckWorkingObjects(SpatialElementViewModel[] workingObjects) {
            var errorElements = new Dictionary<string, InfoElementViewModel>();

            // Все помещения которые
            // избыточные или не окруженные
            var redundantRooms = workingObjects
                .Where(item => item.IsRedundant == true || item.NotEnclosed == true);
            AddElements(InfoElement.RedundantRooms, redundantRooms, errorElements);

            // Все помещения у которых
            // не заполнены обязательные параметры
            foreach(var room in workingObjects) {
                if(room.Room == null) {
                    AddElement(InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomName.Name),
                        null, room, errorElements);
                }

                if(room.RoomGroup == null) {
                    AddElement(
                        InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomGroupName.Name), null,
                        room, errorElements);
                }

                if(room.RoomSection == null) {
                    AddElement(
                        InfoElement.RequiredParams.FormatMessage(ProjectParamsConfig.Instance.RoomSectionName.Name),
                        null, room, errorElements);
                }
            }

            return ShowInfoElementsWindow("Информация", errorElements.Values);
        }

        private bool CanNumerateRooms(object param) {
            if(!int.TryParse(StartNumber, out var value)) {
                ErrorText = "Начальный номер должен быть числом.";
                return false;
            }

            if(value < 1) {
                ErrorText = "Начальный номер должен быть положительным числом.";
                return false;
            }

            if(Phase == null) {
                ErrorText = "Выберите стадию.";
                return false;
            }

            if(IsNumFlats == false && IsNumRooms == false) {
                ErrorText = "Выберите выберете режим работы.";
                return false;
            }

            if(IsNumRooms && IsNumRoomsGroup == false && IsNumRoomsSection == false &&
               IsNumRoomsSectionLevels == false) {
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


            if(!SelectedNumberingOrders.Any() && IsNumRooms) {
                ErrorText = "Настройте список приоритетов нумерации.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private int GetStartNumber() {
            return int.TryParse(StartNumber, out var value) ? value : 0;
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

        private void AddElements(InfoElement infoElement, IEnumerable<IElementViewModel<Element>> elements,
            Dictionary<string, InfoElementViewModel> infoElements) {
            foreach(var element in elements) {
                AddElement(infoElement, null, element, infoElements);
            }
        }

        private void AddElement(InfoElement infoElement, string message, IElementViewModel<Element> element,
            Dictionary<string, InfoElementViewModel> infoElements) {
            if(!infoElements.TryGetValue(infoElement.Message, out var value)) {
                value = new InfoElementViewModel() {
                    Message = infoElement.Message,
                    TypeInfo = infoElement.TypeInfo,
                    Description = infoElement.Description,
                    Elements = new ObservableCollection<MessageElementViewModel>()
                };
                infoElements.Add(infoElement.Message, value);
            }

            value.Elements.Add(new MessageElementViewModel() {Element = element, Description = message});
        }

        private bool ShowInfoElementsWindow(string title, ICollection<InfoElementViewModel> infoElements) {
            if(infoElements.Any()) {
                var window = new InfoElementsWindow() {
                    Title = title,
                    DataContext = new InfoElementsViewModel() {
                        InfoElement = infoElements.FirstOrDefault(),
                        InfoElements = new ObservableCollection<InfoElementViewModel>(infoElements)
                    }
                };

                window.Show();
                return true;
            }

            return false;
        }

        private void SetRoomsNumsConfig() {
            var roomsConfig = RoomsNumsConfig.GetConfig();
            var settings = roomsConfig.GetRoomsNumsSettingsConfig(_revitRepository.DocumentName);
            if(settings == null) {
                return;
            }

            StartNumber = settings.StartNumber ?? "1";
            IsNumFlats = settings.IsNumFlats;
            IsNumRooms = settings.IsNumRooms;
            IsNumRoomsGroup = settings.IsNumRoomsGroup;
            IsNumRoomsSection = settings.IsNumRoomsSection;
            IsNumRoomsSectionLevels = settings.IsNumRoomsSectionLevels;

            if(_revitRepository.GetElement(new ElementId(settings.PhaseElementId)) is Phase phase) {
                if(!(phase == null || Phase?.ElementId == phase.Id)) {
                    Phase = Phases.FirstOrDefault(item => item.ElementId == phase.Id) ?? Phases.FirstOrDefault();
                }
            }

            foreach(var level in Levels.Where(item => settings.Levels.Contains(item.ElementId.IntegerValue))) {
                level.IsSelected = true;
            }

            foreach(var group in Groups.Where(item => settings.Groups.Contains(item.ElementId.IntegerValue))) {
                group.IsSelected = true;
            }

            foreach(var section in Sections.Where(item => settings.Sections.Contains(item.ElementId.IntegerValue))) {
                section.IsSelected = true;
            }
        }

        private void SaveRoomsNumsConfig() {
            var roomsConfig = RoomsNumsConfig.GetConfig();
            var settings = roomsConfig.GetRoomsNumsSettingsConfig(_revitRepository.DocumentName);
            if(settings == null) {
                settings = new RoomsNumsSettings();
                roomsConfig.AddRoomsNumsSettingsConfig(settings);
            }

            settings.StartNumber = StartNumber;
            settings.IsNumFlats = IsNumFlats;
            settings.IsNumRooms = IsNumRooms;
            settings.IsNumRoomsGroup = IsNumRoomsGroup;
            settings.IsNumRoomsSection = IsNumRoomsSection;
            settings.IsNumRoomsSectionLevels = IsNumRoomsSectionLevels;

            settings.SelectedRoomId = _id;
            settings.PhaseElementId = Phase.ElementId.IntegerValue;
            settings.DocumentName = _revitRepository.DocumentName;

            settings.Levels = Levels
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId.IntegerValue)
                .ToList();

            settings.Groups = Groups
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId.IntegerValue)
                .ToList();

            settings.Sections = Sections
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId.IntegerValue)
                .ToList();

            RoomsNumsConfig.SaveConfig(roomsConfig);
        }
    }
}