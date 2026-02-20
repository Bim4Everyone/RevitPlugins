using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Extensions;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class UtpCalculator {
    private const int _minHighFlatHeight = 3400;
    private const int _minBalconyDepth = 1200;
    private const int _minPantryDepth = 1000;
    private const double _minPantryArea = 1.8;

    private readonly ApartmentsProject _project;
    private readonly ApartmentsSettings _settings;
    private readonly ILocalizationService _localizationService;
    private readonly PrioritiesConfig _priorities;
    private readonly RoomConnectionAnalyzer _roomConnectionAnalyzer;

    private readonly bool _hasNullAreas;
    private readonly bool _hasBannedNames;
    private readonly IEnumerable<ElementId> _roomsWithBathFamily;

    private const string _utpYes = "Да";
    private const string _utpNo = "Нет";
    private const string _utpNamesError = "Ошибка имен помещений";
    private const string _utpNullAreasError = "Ошибка нулевых площадей";

    public UtpCalculator(ApartmentsProject project, DeclarationSettings settings, ILocalizationService localizationService) {
        _project = project;
        _settings = (ApartmentsSettings) settings;
        _localizationService = localizationService;

        _hasNullAreas = CheckUtpNullAreas();
        _hasBannedNames = GetBannedUtpRoomNames().Any();
        _roomsWithBathFamily = GetRoomsWithBath();

        _priorities = _settings.PrioritiesConfig;

        _roomConnectionAnalyzer = new RoomConnectionAnalyzer(project, _priorities);
    }

    public void CalculateRoomsForUtp() {
        _roomConnectionAnalyzer.FindConnections();
    }

    public IReadOnlyCollection<WarningViewModel> CheckProjectForUtp() {
        var areaErrorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Warning"),
            Description = _localizationService.GetLocalizedString("WarningWindow.NullAreas"),
            DocumentName = _project.Document.Name
        };

        if(_hasNullAreas) {
            areaErrorListVM.Elements.Add(new WarningElementViewModel(_project.Document.Name,
                _localizationService.GetLocalizedString("WarningWindow.NullAreasInfo")));
        }

        var namesErrorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Warning"),
            Description = _localizationService.GetLocalizedString("WarningsWindow.RoomNameErrors"),
            DocumentName = _project.Document.Name
        };

        if(_hasBannedNames) {
            string names = string.Join(",", GetBannedUtpRoomNames());

            namesErrorListVM.Elements.Add(new WarningElementViewModel(_project.Document.Name,
                _localizationService.GetLocalizedString("WarningsWindow.RoomNameErrorsInfo", names)));
        }

        var bathesErrorListVM = new WarningViewModel(_localizationService) {
            WarningType = _localizationService.GetLocalizedString("WarningWindow.Warning"),
            Description = _localizationService.GetLocalizedString("WarningsWindow.BathErrors"),
            DocumentName = _project.Document.Name
        };

        if(_project.GetBathInstances().Count == 0) {
            bathesErrorListVM.Elements.Add(new WarningElementViewModel(_project.Document.Name,
                _localizationService.GetLocalizedString("WarningsWindow.BathErrorsInfo")));
        }

        return [areaErrorListVM, namesErrorListVM, bathesErrorListVM];
    }

    // УТП Хайфлет.
    // Все помещения в квартире имеют высоту от 3400 мм.
    public string CalculateHighflat(Apartment apartment) {
        foreach(var room in apartment.Rooms) {
            double height = room.GetLengthParamValue(_settings.RoomsHeightParam, _settings.AccuracyForArea);
            if(height < _minHighFlatHeight) {
                return _utpNo;
            }
        }
        return _utpYes;
    }

    // УТП Две ванны.
    // В квартире две и более ванны (или душевые) в разных помещениях санузлов.
    // Помещения санузлов, которые относятся к мастер-спальням не учитываются.
    public string CalculateTwoBathes(Apartment apartment) {
        if(_hasBannedNames) {
            return _utpNamesError;
        }

        int bathAmount = 0;

        bathAmount = apartment.Rooms
            .Select(x => x.RevitRoom.Id)
            .Where(x => !_roomConnectionAnalyzer.CheckIsMasterBathroom(x))
            .Where(x => _roomsWithBathFamily.Contains(x))
            .Count();

        return bathAmount > 1 ? _utpYes : _utpNo;
    }

    // УТП Лоджия/балкон.
    // В квартире есть минимум одно помещение с именем "Лоджия" или "Балкон".
    public string CalculateBalcony(Apartment apartment) {

        int balconies = apartment.GetRoomsByPrior(_priorities.Balcony).Count;
        int loggies = apartment.GetRoomsByPrior(_priorities.Loggia).Count;

        return balconies + loggies > 0 ? _utpYes : _utpNo;
    }

    // УТП Доп. летние помещения.
    // В квартире есть минимум два помещения с именем "Лоджия" или "Балкон".
    public string CalculateExtraSummerRooms(Apartment apartment) {
        int balconies = apartment.GetRoomsByPrior(_priorities.Balcony).Count;
        int loggies = apartment.GetRoomsByPrior(_priorities.Loggia).Count;

        return balconies + loggies > 1 ? _utpYes : _utpNo;
    }

    // УТП Мастер-спальня.
    // В квартире минимум одна жилая комната, соединенная с санузлом.
    // Жилая комната может иметь дверь непосредственно в санузел
    // или сначала в гардеробную, а гардеробная уже в санузел.
    public string CalculateMasterBedroom(Apartment apartment) {
        if(_hasBannedNames) {
            return _utpNamesError;
        }

        foreach(var room in apartment.Rooms) {
            if(_roomConnectionAnalyzer.CheckIsMasterBedroom(room.RevitRoom.Id)) {
                return _utpYes;
            }
        }
        return _utpNo;
    }

    // УТП Терраса.
    // В квартире есть минимум одно помещение с именем "Терраса".
    public string CalculateTerrace(Apartment apartment) {
        int amount = apartment.GetRoomsByPrior(_priorities.Terrace).Count;

        return amount > 0 ? _utpYes : _utpNo;
    }

    // УТП Увеличенная площадь балкона/лоджии.
    // Есть минимум одно помещение с именем "Лоджия" или "Балкон"
    // с глубиной не менее 1200 мм.
    public string CalculateExtraBalconyArea(Apartment apartment) {
        if(_hasNullAreas) {
            return _utpNullAreasError;
        }

        List<RoomElement> summerRooms =
        [
            .. apartment.GetRoomsByPrior(_priorities.Balcony),
            .. apartment.GetRoomsByPrior(_priorities.Loggia),
        ];
        var summerRevitRooms = summerRooms.Select(x => x.RevitRoom).ToList();

        return summerRooms.Any()
            ? ContourChecker
                .CheckAnyRoomSizes(summerRevitRooms, _settings.AccuracyForArea, 0, _minBalconyDepth)
                .GetDescription()
            : _utpNo;
    }

    // УТП Гардеробная.
    // Наличие в квартире минимум одного помещения с именем "Гардеробная".
    // Одна сторона помещения должна быть не менее 1000 мм, площадь от 1,8 м2.
    // Помещения, которые связаны дверью с жилой комнатой не учитываются.
    public string CalculatePantry(Apartment apartment) {
        if(_hasNullAreas) {
            return _utpNullAreasError;
        }
        if(_hasBannedNames) {
            return _utpNamesError;
        }

        var pantriesWithoutBedrooms = apartment
            .GetRoomsByPrior(_priorities.Pantry)
            .Select(x => x.RevitRoom)
            .Where(x => !_roomConnectionAnalyzer.CheckIsPantryWithBedroom(x.Id))
            .Where(x => ContourChecker.CheckArea(x, _settings.AccuracyForArea, _minPantryArea))
            .ToList();

        return pantriesWithoutBedrooms.Any()
            ? ContourChecker
                .CheckAnyRoomSizes(pantriesWithoutBedrooms, _settings.AccuracyForArea, _minPantryDepth)
                .GetDescription()
            : _utpNo;
    }

    // УТП Постирочная.
    // В квартире есть минимум одно помещение с именем "Постирочная".
    public string CalculateLaundry(Apartment apartment) {
        bool hasLaundries = apartment
            .GetRoomsByPrior(_priorities.Laundry)
            .Any();

        return hasLaundries ? _utpYes : _utpNo;
    }

    private bool CheckUtpNullAreas() {
        return _project
            .Rooms
            .Where(x => x.AreaRevit < 0.1)
            .Any();
    }

    private IEnumerable<string> GetBannedUtpRoomNames() {
        return _project
            .Rooms
            .Select(x => x.Name)
            .Where(x => _settings.BannedRoomNames.Contains(x, StringComparer.OrdinalIgnoreCase))
            .Distinct()
            .ToList();
    }

    private IEnumerable<ElementId> GetRoomsWithBath() {
        return _project
            .GetBathInstances()
            .Select(x => x.get_Room(_project.Phase))
            .Distinct()
            .Select(x => x?.Id)
            .ToList();
    }
}
