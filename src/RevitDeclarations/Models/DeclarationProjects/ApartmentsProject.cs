using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class ApartmentsProject : DeclarationProject {
    private UtpCalculator _utpCalculator;

    public ApartmentsProject(RevitDocumentViewModel document,
                            RevitRepository revitRepository,
                            DeclarationSettings settings,
                            ILocalizationService localizationService) 
        : base(document, revitRepository, settings, localizationService) {

        var paramProvider = new RoomParamProvider(settings);
        _roomGroups = revitRepository
            .GroupRooms(_rooms, settings)
            .Select(x => new Apartment(x, settings, paramProvider))
            .ToList();
    }

    /// <summary>
    /// У каждого помещения одной квартиры должны быть одинаковые значения параметров,
    /// в которых хранятся общие площади этой квартиры.
    /// Проверяется общая площадь, жилая, без летних помещений и площадь с коэффициентом.
    /// </summary>
    /// <returns></returns>
    public WarningViewModel CheckRoomAreasEquality() {
        var errorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Error"),
            Description = _localizationService.GetLocalizedString("WarningsWindow.DiffInAreas"),
            DocumentName = _document.Name
        };

        foreach(Apartment apartment in _roomGroups) {
            if(!apartment.CheckEqualityOfRoomAreas()) {
                string apartInfo = _localizationService
                    .GetLocalizedString("WarningsWindow.ErrorRoomGroup", apartment.Number, apartment.Level);                
                string apartAreas = _localizationService.GetLocalizedString("WarningsWindow.ErrorRoomAreasInfo");
                errorListVM.Elements.Add(new WarningElementViewModel(apartInfo, apartAreas));
            }
        }

        return errorListVM;
    }

    public WarningViewModel CheckActualApartmentAreas() {
        var errorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Warning"),
            Description = _localizationService.GetLocalizedString("WarningsWindow.ErrorRoomNotActualAreasInfo"),
            DocumentName = _document.Name
        };

        foreach(Apartment apartment in _roomGroups) {
            if(!apartment.CheckActualApartmentAreas()) {
                string apartInfo = _localizationService
                    .GetLocalizedString("WarningsWindow.ErrorRoomGroup", apartment.Number, apartment.Level);
                string apartAreas = _localizationService.GetLocalizedString("WarningsWindow.ErrorApartsNotActualAreasInfo");
                errorListVM.Elements.Add(new WarningElementViewModel(apartInfo, apartAreas));
            }
        }

        return errorListVM;
    }

    public IReadOnlyCollection<WarningViewModel> CheckUtpWarnings() {
        _utpCalculator = new UtpCalculator(this, _settings, _localizationService);
        return _utpCalculator.CheckProjectForUtp();
    }

    public void CalculateUtpForApartments() {
        _utpCalculator.CalculateRoomsForUtp();

        foreach(Apartment apartment in _roomGroups) {
            apartment.CalculateUtp(_utpCalculator);
        }
    }

    public IReadOnlyCollection<FamilyInstance> GetDoors() {
        return _revitRepository
            .GetDoorsOnPhase(_document.Document, _phase);
    }

    public IReadOnlyCollection<CurveElement> GetCurveSeparators() {
        return _revitRepository
            .GetRoomSeparationLinesOnPhase(_document.Document, _phase);
    }

    public IReadOnlyCollection<FamilyInstance> GetBathInstances() {
        return _revitRepository.
            GetBathInstancesOnPhase(_document.Document, _phase);
    }
}
