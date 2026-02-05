using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.Commands;
using RevitRooms.Commands.Numerates;
using RevitRooms.Models;
using RevitRooms.Services;
using RevitRooms.Views;

namespace RevitRooms.ViewModels.RoomsNums;
internal abstract class RevitRoomNumsViewModel : BaseViewModel, INumberingOrder {
    public Guid _id;
    protected readonly RevitRepository _revitRepository;
    protected readonly RoomsNumsConfig _roomsNumsConfig;
    protected readonly IMessageBoxService _messageBoxService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ErrorWindowService _errorWindowService;
    protected readonly PluginSettings _pluginSettings;

    private string _errorText;
    private string _prefix;
    private string _suffix;
    private bool _isNumFlats;
    private bool _isNumRooms;
    private bool _isNumRoomsGroup;
    private bool _isNumRoomsSection;
    private bool _isNumRoomsSectionLevels;
    private string _startNumber;

    private PhaseViewModel _phase;
    private ObservableCollection<PhaseViewModel> _phases;
    private ObservableCollection<SpatialElementViewModel> _spatialElements;
    private ObservableCollection<LevelViewModel> _levels;
    private ObservableCollection<IElementViewModel<Element>> _groups;
    private ObservableCollection<IElementViewModel<Element>> _sections;
    private ObservableCollection<NumberingOrderViewModel> _numberingOrders;
    private ObservableCollection<NumberingOrderViewModel> _selectedNumberingOrders;

    public System.Windows.Window ParentWindow { get; set; }

    public RevitRoomNumsViewModel(RevitRepository revitRepository, 
                                  RoomsNumsConfig roomsNumsConfig,
                                  IMessageBoxService messageBoxService,
                                  ILocalizationService localizationService,
                                  NumOrderWindowService numOrderWindowService,
                                  ErrorWindowService errorWindowService) {
        _revitRepository = revitRepository;
        _roomsNumsConfig = roomsNumsConfig;
        _messageBoxService = messageBoxService;
        _localizationService = localizationService;
        _errorWindowService = errorWindowService;
        _pluginSettings = _roomsNumsConfig.PluginSettings;

        var additionalPhases = _revitRepository.GetAdditionalPhases()
            .Select(item => new PhaseViewModel(item, _revitRepository))
            .ToArray();

        SpatialElements = [.. GetSpatialElements()
            .Where(item => item.Phase != null)
            .Where(item => !additionalPhases.Contains(item.Phase))
            .Where(item => item.IsPlaced)];

        Phases = [.. GetPhases()];
        Levels = [.. GetLevels()];
        Groups = [.. GetGroups()];
        Sections = [.. GetSections()];

        NumberingOrders = [.. GetNumberingOrders().Where(item => item.Order == 0)];
        SelectedNumberingOrders = [.. GetNumberingOrders().Where(item => item.Order > 0)];

        StartNumber = "1";
        Phase = Phases.FirstOrDefault();        

        NumerateRoomsCommand = new RelayCommand(NumerateRooms, CanNumerateRooms);

        UpOrderCommand = new UpOrderCommand(this);
        DownOrderCommand = new DownOrderCommand(this);
        AddOrderCommand = new AddOrderCommand(this, numOrderWindowService);
        RemoveOrderCommand = new RemoveOrderCommand(this);
        SaveOrderCommand = new SaveOrderCommand(this, _revitRepository, localizationService);

        LoadPluginConfig();
    }

    public ICommand LoadViewCommand { get; }
    public ICommand NumerateRoomsCommand { get; }

    public ICommand UpOrderCommand { get; }
    public ICommand DownOrderCommand { get; }
    public ICommand AddOrderCommand { get; }
    public ICommand RemoveOrderCommand { get; }
    public ICommand SaveOrderCommand { get; }

    public string Name { get; set; }
    protected abstract IEnumerable<SpatialElementViewModel> GetSpatialElements();

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }

    public string StartNumber {
        get => _startNumber;
        set => RaiseAndSetIfChanged(ref _startNumber, value);
    }

    public string Suffix {
        get => _suffix;
        set => RaiseAndSetIfChanged(ref _suffix, value);
    }

    public bool IsNumFlats {
        get => _isNumFlats;
        set => RaiseAndSetIfChanged(ref _isNumFlats, value);
    }

    public bool IsNumRooms {
        get => _isNumRooms;
        set => RaiseAndSetIfChanged(ref _isNumRooms, value);
    }

    public bool IsNumRoomsGroup {
        get => _isNumRoomsGroup;
        set => RaiseAndSetIfChanged(ref _isNumRoomsGroup, value);
    }

    public bool IsNumRoomsSection {
        get => _isNumRoomsSection;
        set => RaiseAndSetIfChanged(ref _isNumRoomsSection, value);
    }

    public bool IsNumRoomsSectionLevels {
        get => _isNumRoomsSectionLevels;
        set => RaiseAndSetIfChanged(ref _isNumRoomsSectionLevels, value);
    }

    public ObservableCollection<PhaseViewModel> Phases {
        get => _phases;
        set => RaiseAndSetIfChanged(ref _phases, value);
    }
    public PhaseViewModel Phase {
        get => _phase;
        set => RaiseAndSetIfChanged(ref _phase, value);
    }

    public ObservableCollection<SpatialElementViewModel> SpatialElements {
        get => _spatialElements;
        set => RaiseAndSetIfChanged(ref _spatialElements, value);
    }

    public ObservableCollection<LevelViewModel> Levels {
        get => _levels;
        set => RaiseAndSetIfChanged(ref _levels, value);
    }

    public ObservableCollection<IElementViewModel<Element>> Groups {
        get => _groups;
        set => RaiseAndSetIfChanged(ref _groups, value);
    }

    public ObservableCollection<IElementViewModel<Element>> Sections {
        get => _sections;
        set => RaiseAndSetIfChanged(ref _sections, value);
    }

    public ObservableCollection<NumberingOrderViewModel> NumberingOrders {
        get => _numberingOrders;
        set => RaiseAndSetIfChanged(ref _numberingOrders, value);
    }

    public ObservableCollection<NumberingOrderViewModel> SelectedNumberingOrders {
        get => _selectedNumberingOrders;
        set => RaiseAndSetIfChanged(ref _selectedNumberingOrders, value);
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
                    SpatialElements.Select(room => room.Element), _pluginSettings))
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

        int startNumber = GetStartNumber();

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
            ParentWindow.Close();
            return;
        }

        var orderedObjects = workingObjects
            .Where(item => item.RoomGroup != null && groups.Contains(item.RoomGroup.Id))
            .Where(item => item.RoomSection != null && sections.Contains(item.RoomSection.Id))
            .ToArray();

        string[] notFoundNames = GetNotFoundNames(orderedObjects);
        if(notFoundNames.Length > 0) {
            ShowNotFoundNames(notFoundNames);
            return;
        }

        using(var window = SetupProgressDialog(orderedObjects)) {
            window.Show();
            if(IsNumFlats) {
                var numerateCommand =
                    new NumFlatsCommand(_revitRepository, _localizationService) { Start = startNumber, Prefix = Prefix, Suffix = Suffix };
                numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
            } else {
                UpdateNumeringOrder();

                var selectedOrder = SelectedNumberingOrders
                    .ToDictionary(
                        item => item.ElementId,
                        item => item.Order);

                if(IsNumRoomsGroup) {
                    var numerateCommand =
                        new NumSectionGroup(_revitRepository, _localizationService, selectedOrder) {
                            Start = startNumber,
                            Prefix = Prefix,
                            Suffix = Suffix
                        };
                    numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                } else if(IsNumRoomsSection) {
                    var numerateCommand =
                        new NumSectionCommand(_revitRepository, _localizationService, selectedOrder) {
                            Start = startNumber,
                            Prefix = Prefix,
                            Suffix = Suffix
                        };
                    numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                } else if(IsNumRoomsSectionLevels) {
                    var numerateCommand =
                        new NumerateSectionLevel(_revitRepository, _localizationService, selectedOrder) {
                            Start = startNumber,
                            Prefix = Prefix,
                            Suffix = Suffix
                        };
                    numerateCommand.Numerate(orderedObjects, window.CreateProgress(), window.CreateCancellationToken());
                } else {
                    string excMessage = _localizationService.GetLocalizedString("UnknownSettings");
                    throw new InvalidOperationException(excMessage);
                }
            }
        }

        SavePluginConfig();

        ParentWindow.DialogResult = true;
        ParentWindow.Close();
        string message = _localizationService.GetLocalizedString("TaskDialog.Result");
        string title = _localizationService.GetLocalizedString("TaskDialog.Information");
        _messageBoxService.Show(message, title);
    }

    private IProgressDialogService SetupProgressDialog(SpatialElementViewModel[] orderedObjects) {
        var service = GetPlatformService<IProgressDialogService>();
        service.StepValue = 10;
        service.MaxValue = orderedObjects.Length;
        service.DisplayTitleFormat =  _localizationService.GetLocalizedString("RoomNumsWindow.Numerating");
        return service;
    }

    private bool ShowNotFoundNames(string[] notFoundNames) {
        string message = _localizationService.GetLocalizedString("WarningsWindow.NoPriorities");
        string title = _localizationService.GetLocalizedString("RoomsWindow.Title");

        var taskDialog = new TaskDialog(title) {
            AllowCancellation = true,
            MainInstruction = message,
            MainContent = " - " + string.Join(Environment.NewLine + " - ", notFoundNames)
        };

        string addNewMessage = _localizationService.GetLocalizedString("WarningsWindow.AddPriorities");
        string exitMessage = _localizationService.GetLocalizedString("WarningsWindow.Exit");

        taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, addNewMessage);
        taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, exitMessage);
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
        return IsNumFlats
            ? (new string[0])
            : orderedObjects
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
        var errors = new Dictionary<string, WarningViewModel>();

        // Все помещения которые
        // избыточные или не окруженные
        var redundantRooms = workingObjects
            .Where(item => item.IsRedundant == true || item.NotEnclosed == true);
        AddElements(WarningInfo.GetRedundantRooms(_localizationService), redundantRooms, errors);

        // Все помещения у которых
        // не заполнены обязательные параметры
        foreach(var room in workingObjects) {
            if(room.Room == null) {
                AddElement(WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomName.Name),
                    null, room, errors);
            }

            if(room.RoomGroup == null) {
                AddElement(
                    WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomGroupName.Name), null,
                    room, errors);
            }

            if(room.RoomSection == null) {
                AddElement(
                    WarningInfo.GetRequiredParams(_localizationService, ProjectParamsConfig.Instance.RoomSectionName.Name),
                    null, room, errors);
            }
        }

        // Все многоуровневые помещения
        var multiLevelRooms = workingObjects
            .Where(item => !string.IsNullOrEmpty(item.RoomMultilevelGroup))
            .GroupBy(item => new { item.RoomMultilevelGroup, item.LevelName });

        foreach(var multiLevelRoomGroup in multiLevelRooms) {
            bool notSameValue = multiLevelRoomGroup
                .GroupBy(item => item.IsRoomMainLevel)
                .Count() > 1;

            if(notSameValue) {
                AddElements(WarningInfo.GetErrorMultiLevelRoom(_localizationService), multiLevelRoomGroup, errors);
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
                AddElements(WarningInfo.GetErrorMultiLevelRoom(_localizationService), multiLevelRoomGroup, errors);
            }
        }

        bool showWarnings = true;
        return _errorWindowService.ShowNoticeWindow(showWarnings, [.. errors.Values]);
    }

    private bool CanNumerateRooms(object param) {
        if(!int.TryParse(StartNumber, out int value)) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningStartNumber");
            return false;
        }

        if(value < 1) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningPositiveStartNumber");
            return false;
        }

        if(Phase == null) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectPhase");
            return false;
        }

        if(IsNumFlats == false && IsNumRooms == false) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectMode");
            return false;
        }

        if(IsNumRooms && IsNumRoomsGroup == false && IsNumRoomsSection == false &&
           IsNumRoomsSectionLevels == false) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectModeRooms");
            return false;
        }

        if(!Sections.Any(item => item.IsSelected)) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectSection");
            return false;
        }

        if(!Groups.Any(item => item.IsSelected)) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectGroup");
            return false;
        }

        if(!Levels.Any(item => item.IsSelected)) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSelectLevel");
            return false;
        }


        if(!SelectedNumberingOrders.Any() && IsNumRooms) {
            ErrorText = _localizationService.GetLocalizedString("RoomNumsWindow.WarningSetPriorities");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private int GetStartNumber() {
        return int.TryParse(StartNumber, out int value) ? value : 0;
    }

    private void AddElements(WarningInfo warningInfo, 
                             IEnumerable<IElementViewModel<Element>> elements,
                             Dictionary<string, WarningViewModel> warningElements) {
        foreach(var element in elements) {
            AddElement(warningInfo, null, element, warningElements);
        }
    }

    private void AddElement(WarningInfo warningInfo, 
                            string message, 
                            IElementViewModel<Element> element,
                            Dictionary<string, WarningViewModel> warningElements) {
        if(!warningElements.TryGetValue(warningInfo.Message, out var value)) {
            value = new WarningViewModel(_localizationService) {
                Message = warningInfo.Message,
                TypeInfo = warningInfo.TypeInfo,
                Description = warningInfo.Description,
                Elements = []
            };
            warningElements.Add(warningInfo.Message, value);
        }

        value.Elements.Add(new WarningElementViewModel() { Element = element, Description = message });
    }

    private void LoadPluginConfig() {
        var settings = _roomsNumsConfig.GetSettings(_revitRepository.DocumentName);

        StartNumber = settings?.StartNumber ?? "1";
        IsNumFlats = settings?.IsNumFlats ?? default;
        IsNumRooms = settings?.IsNumRooms ?? default;
        IsNumRoomsGroup = settings?.IsNumRoomsGroup ?? default;
        IsNumRoomsSection = settings?.IsNumRoomsSection ?? default;
        IsNumRoomsSectionLevels = settings?.IsNumRoomsSectionLevels ?? default;

        if(_revitRepository.GetElement(settings?.PhaseElementId ?? ElementId.InvalidElementId) is Phase phase) {
            Phase = Phases.FirstOrDefault(item => item.ElementId == phase.Id) ?? Phases.FirstOrDefault();
        }

        foreach(var level in Levels
                    .Where(item => settings?.Levels.Contains(item.ElementId) == true)) {
            level.IsSelected = true;
        }

        foreach(var group in Groups
                    .Where(item => settings?.Groups.Contains(item.ElementId) == true)) {
            group.IsSelected = true;
        }

        foreach(var section in Sections
                    .Where(item => settings?.Sections.Contains(item.ElementId) == true)) {
            section.IsSelected = true;
        }
    }

    private void SavePluginConfig() {
        var settings = _roomsNumsConfig.GetSettings(_revitRepository.DocumentName)
                                     ?? _roomsNumsConfig.AddSettings(_revitRepository.DocumentName);

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

        _roomsNumsConfig.SaveProjectConfig();
    }
}
