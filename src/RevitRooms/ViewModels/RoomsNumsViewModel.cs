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

using dosymep.SimpleServices;

using RevitRooms.Commands.Numerates;

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
        private PhaseViewModel _phase;
        private ObservableCollection<PhaseViewModel> _phases;
        private ObservableCollection<SpatialElementViewModel> _spatialElements;
        private ObservableCollection<LevelViewModel> _levels;
        private ObservableCollection<IElementViewModel<Element>> _groups;
        private ObservableCollection<IElementViewModel<Element>> _sections;
        private ObservableCollection<NumberingOrderViewModel> _numberingOrders;
        private ObservableCollection<NumberingOrderViewModel> _selectedNumberingOrders;

        public System.Windows.Window ParentWindow { get; set; }

        public RoomsNumsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            LoadViewCommand = RelayCommand.Create(LoadView);
            NumerateRoomsCommand = new RelayCommand(NumerateRooms, CanNumerateRooms);

            UpOrderCommand = new UpOrderCommand(this);
            DownOrderCommand = new DownOrderCommand(this);
            AddOrderCommand = new AddOrderCommand(this);
            RemoveOrderCommand = new RemoveOrderCommand(this);
            SaveOrderCommand = new SaveOrderCommand(this, _revitRepository);
        }

        private void LoadView() {
            PhaseViewModel[] additionalPhases = _revitRepository.GetAdditionalPhases()
                .Select(item => new PhaseViewModel(item, _revitRepository))
                .ToArray();

            SpatialElements = new ObservableCollection<SpatialElementViewModel>(GetSpatialElements()
                .Where(item => item.Phase != null)
                .Where(item => !additionalPhases.Contains(item.Phase))
                .Where(item => item.IsPlaced));

            Phases = new ObservableCollection<PhaseViewModel>(GetPhases());
            Levels = new ObservableCollection<LevelViewModel>(GetLevels());
            Groups = new ObservableCollection<IElementViewModel<Element>>(GetGroups());
            Sections = new ObservableCollection<IElementViewModel<Element>>(GetSections());

            NumberingOrders = new ObservableCollection<NumberingOrderViewModel>(
                GetNumberingOrders()
                    .Where(item => item.Order == 0));

            SelectedNumberingOrders = new ObservableCollection<NumberingOrderViewModel>(
                GetNumberingOrders()
                    .Where(item => item.Order > 0));

            StartNumber = "1";
            Phase = Phases.FirstOrDefault();
            
            LoadPluginConfig();
        }

        public string Name { get; set; }
        protected abstract IEnumerable<SpatialElementViewModel> GetSpatialElements();

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

        public ICommand LoadViewCommand { get; }
        public ICommand NumerateRoomsCommand { get; }

        public ICommand UpOrderCommand { get; }
        public ICommand DownOrderCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand RemoveOrderCommand { get; }
        public ICommand SaveOrderCommand { get; }

        public PhaseViewModel Phase {
            get => _phase;
            set => this.RaiseAndSetIfChanged(ref _phase, value);
        }

        public ObservableCollection<PhaseViewModel> Phases {
            get => _phases;
            set => this.RaiseAndSetIfChanged(ref _phases, value);
        }

        public ObservableCollection<SpatialElementViewModel> SpatialElements {
            get => _spatialElements;
            set => this.RaiseAndSetIfChanged(ref _spatialElements, value);
        }

        public ObservableCollection<LevelViewModel> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }

        public ObservableCollection<IElementViewModel<Element>> Groups {
            get => _groups;
            set => this.RaiseAndSetIfChanged(ref _groups, value);
        }

        public ObservableCollection<IElementViewModel<Element>> Sections {
            get => _sections;
            set => this.RaiseAndSetIfChanged(ref _sections, value);
        }

        public ObservableCollection<NumberingOrderViewModel> NumberingOrders {
            get => _numberingOrders;
            set => this.RaiseAndSetIfChanged(ref _numberingOrders, value);
        }

        public ObservableCollection<NumberingOrderViewModel> SelectedNumberingOrders {
            get => _selectedNumberingOrders;
            set => this.RaiseAndSetIfChanged(ref _selectedNumberingOrders, value);
        }

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

            SpatialElementViewModel[] orderedObjects = workingObjects
                .Where(item => item.RoomGroup != null && groups.Contains(item.RoomGroup.Id))
                .Where(item => item.RoomSection != null && sections.Contains(item.RoomSection.Id))
                .ToArray();

            var notFoundNames = GetNotFoundNames(orderedObjects);
            if(notFoundNames.Length > 0) {
                ShowNotFoundNames(notFoundNames);
                return;
            }

            using(var window = SetupProgressDialog(orderedObjects)) {
                window.Show();
                if(IsNumFlats) {
                    var numerateCommand =
                        new NumFlatsCommand(_revitRepository) {Start = startNumber, Prefix = Prefix, Suffix = Suffix};
                    numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                } else {
                    UpdateNumeringOrder();

                    var selectedOrder = SelectedNumberingOrders
                        .ToDictionary(
                            item => item.ElementId,
                            item => item.Order);

                    if(IsNumRoomsGroup) {
                        var numerateCommand =
                            new NumSectionGroup(_revitRepository, selectedOrder) {
                                Start = startNumber, Prefix = Prefix, Suffix = Suffix
                            };
                        numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                    } else if(IsNumRoomsSection) {
                        var numerateCommand =
                            new NumSectionCommand(_revitRepository, selectedOrder) {
                                Start = startNumber, Prefix = Prefix, Suffix = Suffix
                            };
                        numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                    } else if(IsNumRoomsSectionLevels) {
                        var numerateCommand =
                            new NumerateSectionLevel(_revitRepository, selectedOrder) {
                                Start = startNumber, Prefix = Prefix, Suffix = Suffix
                            };
                        numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                    } else {
                        throw new InvalidOperationException("Выбран неизвестный режим работы.");
                    }
                }
            }

            SavePluginConfig();

            ParentWindow.DialogResult = true;
            ParentWindow.Close();
            TaskDialog.Show("Предупреждение!", "Расчет завершен!");
        }

        private IProgressDialogService SetupProgressDialog(SpatialElementViewModel[] orderedObjects) {
            var service = GetPlatformService<IProgressDialogService>();
            service.StepValue = 10;
            service.MaxValue = orderedObjects.Length;
            service.DisplayTitleFormat = "Нумерация [{0}]\\{1}]";
            return service;
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

            // Все многоуровневые помещения
            var multiLevelRooms = workingObjects
                .Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup))
                .GroupBy(item => new {item.RoomMultilevelGroup, item.LevelName});

            foreach(var multiLevelRoomGroup in multiLevelRooms) {
                bool notSameValue = multiLevelRoomGroup
                    .GroupBy(item => item.IsRoomMainLevel)
                    .Count() > 1;

                if(notSameValue) {
                    AddElements(InfoElement.ErrorMultiLevelRoom, multiLevelRoomGroup, errorElements);
                }
            }

            var newMultiLevelRooms = workingObjects
                .Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup))
                .GroupBy(item => item.RoomMultilevelGroup);

            foreach(var multiLevelRoomGroup in newMultiLevelRooms) {
                bool notSameValue = multiLevelRoomGroup
                    .GroupBy(item => item.IsRoomMainLevel)
                    .Count() != 2;

                if(notSameValue) {
                    AddElements(InfoElement.ErrorMultiLevelRoom, multiLevelRoomGroup, errorElements);
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

        private void LoadPluginConfig() {
            RoomsNumsConfig roomsConfig = RoomsNumsConfig.GetPluginConfig();
            RoomsNumsSettings settings = roomsConfig.GetSettings(_revitRepository.DocumentName);

            StartNumber = settings?.StartNumber ?? "1";
            IsNumFlats = settings?.IsNumFlats ?? default;
            IsNumRooms = settings?.IsNumRooms ?? default;
            IsNumRoomsGroup = settings?.IsNumRoomsGroup ?? default;
            IsNumRoomsSection =settings?.IsNumRoomsSection ?? default;
            IsNumRoomsSectionLevels =settings?.IsNumRoomsSectionLevels ?? default;

            if(_revitRepository.GetElement(settings?.PhaseElementId ?? ElementId.InvalidElementId) is Phase phase) {
                Phase = Phases.FirstOrDefault(item => item.ElementId == phase.Id) ?? Phases.FirstOrDefault();
            }

            foreach(LevelViewModel level in Levels
                        .Where(item => settings?.Levels.Contains(item.ElementId) == true)) {
                level.IsSelected = true;
            }

            foreach(IElementViewModel<Element> group in Groups
                        .Where(item => settings?.Groups.Contains(item.ElementId) == true)) {
                group.IsSelected = true;
            }

            foreach(IElementViewModel<Element> section in Sections
                        .Where(item => settings?.Sections.Contains(item.ElementId) == true)) {
                section.IsSelected = true;
            }
        }

        private void SavePluginConfig() {
            RoomsNumsConfig roomsConfig = RoomsNumsConfig.GetPluginConfig();
            RoomsNumsSettings settings = roomsConfig.GetSettings(_revitRepository.DocumentName)
                                         ?? roomsConfig.AddSettings(_revitRepository.DocumentName);

            settings.StartNumber = StartNumber;
            settings.IsNumFlats = IsNumFlats;
            settings.IsNumRooms = IsNumRooms;
            settings.IsNumRoomsGroup = IsNumRoomsGroup;
            settings.IsNumRoomsSection = IsNumRoomsSection;
            settings.IsNumRoomsSectionLevels = IsNumRoomsSectionLevels;

            settings.SelectedRoomId = _id;
            settings.PhaseElementId = Phase.ElementId;
            settings.DocumentName = _revitRepository.DocumentName;

            settings.Levels = Levels
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId)
                .ToList();

            settings.Groups = Groups
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId)
                .ToList();

            settings.Sections = Sections
                .Where(item => item.IsSelected)
                .Select(item => item.ElementId)
                .ToList();

            roomsConfig.SaveProjectConfig();
        }
    }
}