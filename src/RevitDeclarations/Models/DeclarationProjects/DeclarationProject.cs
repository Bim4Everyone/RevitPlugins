using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal abstract class DeclarationProject {
    protected readonly RevitDocumentViewModel _document;
    protected readonly RevitRepository _revitRepository;
    protected readonly DeclarationSettings _settings;
    protected readonly ILocalizationService _localizationService;

    protected readonly Phase _phase;

    protected readonly IEnumerable<RoomElement> _rooms;
    protected IReadOnlyCollection<RoomGroup> _roomGroups;

    public DeclarationProject(RevitDocumentViewModel document,
                              RevitRepository revitRepository,
                              DeclarationSettings settings,
                              ILocalizationService localizationService) {
        _document = document;
        _revitRepository = revitRepository;
        _settings = settings;
        _localizationService = localizationService;

        _phase = revitRepository.GetPhaseByName(document.Document, _settings.SelectedPhase.Name);

        _rooms = revitRepository.GetRoomsOnPhase(document.Document, _phase, settings);
        _rooms = FilterDeclarationRooms(_rooms);
    }

    public RevitDocumentViewModel Document => _document;
    public Phase Phase => _phase;

    /// <summary>Отфильтрованные помещения для выгрузки</summary>
    public IEnumerable<RoomElement> Rooms => _rooms;
    public IReadOnlyCollection<RoomGroup> RoomGroups => _roomGroups;

    private IReadOnlyCollection<RoomElement> FilterDeclarationRooms(IEnumerable<RoomElement> rooms) {
        var filterParam = _settings.FilterRoomsParam;
        string[] filterValues = _settings.FilterRoomsValues;
        var strComparer = StringComparer.OrdinalIgnoreCase;

        return rooms
            .Where(x => filterValues.Contains(x.GetTextParamValue(filterParam), strComparer))
            .ToList();
    }

    public WarningViewModel CheckRoomGroupsInProject() {
        var errorListVM = new WarningViewModel(_localizationService) {
            WarningType = "Ошибка",
            Description = "В проекте отсутствуют необходимые группы помещений на выбранной стадии",
            DocumentName = _document.Name
        };

        if(_roomGroups.Count == 0) {
            errorListVM.Elements = [
                new WarningElementViewModel(_settings.SelectedPhase.Name, "Отсутствуют группы помещений")
            ];
        }

        return errorListVM;
    }

    public WarningViewModel CheckActualRoomAreas() {
        var errorListVM = new WarningViewModel(_localizationService) {
            WarningType = "Предупреждение",
            Description = "Не актуальные площади помещений, рассчитанные квартирографией",
            DocumentName = _document.Name
        };

        foreach(var roomGroup in _roomGroups) {
            if(!roomGroup.CheckActualRoomAreas()) {
                string groupInfo = $"Группа помещений № {roomGroup.Number} на этаже {roomGroup.Level}";
                string groupAreas = "Площади помещений, рассчитанные квартирографией " +
                    "отличаются от актуальной системной площадей помещения.";
                errorListVM.Elements.Add(new WarningElementViewModel(groupInfo, groupAreas));
            }
        }

        return errorListVM;
    }
}
