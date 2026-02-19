using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class ApartmentsProject : DeclarationProject {
    private UtpCalculator _utpCalculator;

    public ApartmentsProject(RevitDocumentViewModel document,
                            RevitRepository revitRepository,
                            DeclarationSettings settings) : base(document, revitRepository, settings) {

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
    public ErrorsListViewModel CheckRoomAreasEquality() {
        var errorListVM = new ErrorsListViewModel() {
            ErrorType = "Ошибка",
            Description = "Площади, рассчитанные квартирографией отличаются в пределах квартиры",
            DocumentName = _document.Name
        };

        foreach(Apartment apartment in _roomGroups) {
            if(!apartment.CheckEqualityOfRoomAreas()) {
                string apartInfo = $"Группа помещений № {apartment.Number} на этаже {apartment.Level}";
                string apartAreas = "Площади квартиры (без коэффициента/с коэффициентом/жилая/без ЛП) " +
                    "должны быть одинаковыми для каждого помещения квартиры";
                errorListVM.Errors.Add(new ErrorElement(apartInfo, apartAreas));
            }
        }

        return errorListVM;
    }

    public ErrorsListViewModel CheckActualApartmentAreas() {
        var errorListVM = new ErrorsListViewModel() {
            ErrorType = "Предупреждение",
            Description = "Не актуальные площади квартир, рассчитанные квартирографией",
            DocumentName = _document.Name
        };

        foreach(Apartment apartment in _roomGroups) {
            if(!apartment.CheckActualApartmentAreas()) {
                string apartInfo = $"Группа помещений № {apartment.Number} на этаже {apartment.Level}";
                string apartAreas = "Площади квартиры, рассчитанные квартирографией " +
                    "отличаются от суммы актуальных системных площадей этой квартиры. " +
                    "Проверьте общую площадь квартиры, площадь с коэффициентом, " +
                    "площадь жилых помещений и площадь без летних помещений";
                errorListVM.Errors.Add(new ErrorElement(apartInfo, apartAreas));
            }
        }

        return errorListVM;
    }

    public IReadOnlyCollection<ErrorsListViewModel> CheckUtpWarnings() {
        _utpCalculator = new UtpCalculator(this, _settings);
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
