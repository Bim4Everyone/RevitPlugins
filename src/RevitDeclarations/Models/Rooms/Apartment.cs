using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal class Apartment : RoomGroup {
    // Словарь для группировки помещений, входящих в исходные приоритеты
    private readonly Dictionary<string, List<RoomElement>> _mainRooms;
    // Словарь для группировки помещений, не входящих в исходные приоритеты
    private readonly Dictionary<string, List<RoomElement>> _nonConfigRooms;

    private readonly ApartmentsSettings _settings;

    private double _areaMainRevit;
    private double _areaCoefRevit;
    private double _areaLivingRevit;
    private double _areaNonSummerRevit;

    public Apartment(IEnumerable<RoomElement> rooms,
                     DeclarationSettings settings,
                     RoomParamProvider paramProvider)
        : base(rooms, settings, paramProvider) {
        _settings = (ApartmentsSettings) settings;
        _mainRooms = new Dictionary<string, List<RoomElement>>(_strComparer);
        _nonConfigRooms = new Dictionary<string, List<RoomElement>>(_strComparer);

        foreach(var room in _rooms) {
            if(settings.MainRoomNames.Contains(room.Name, _strComparer)) {
                AddToDictionary(_mainRooms, room);
            } else {
                AddToDictionary(_nonConfigRooms, room);
            }
        }

        CalculateRevitAreas();
    }

    [JsonProperty("full_number")]
    public string FullNumber => _firstRoom.GetTextParamValue(_settings.ApartmentFullNumberParam);

    [JsonProperty("type")]
    public override string Department => _paramProvider.GetDepartment(_firstRoom, "Квартира");
    [JsonProperty("area_k")]
    public double AreaCoef => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaCoefParam, _accuracyForArea);
    [JsonProperty("area_living")]
    public double AreaLiving => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaLivingParam, _accuracyForArea);
    [JsonProperty("area_non_summer")]
    public double AreaNonSummer => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaNonSumParam, _accuracyForArea);
    [JsonProperty("room_size")]
    public double RoomsAmount => _firstRoom.GetIntAndCurrencyParamValue(_settings.RoomsAmountParam);
    [JsonProperty("building_number")]
    public string BuildingNumber => _firstRoom.GetTextParamValue(_settings.BuildingNumberParam);
    [JsonProperty("construction_works")]
    public string ConstrWorksNumber => _firstRoom.GetTextParamValue(_settings.ConstrWorksNumberParam);

    [JsonProperty("ceiling_height")]
    public double RoomsHeight => _firstRoom.GetLengthParamValue(_settings.RoomsHeightParam, _accuracyForLength);

    [JsonIgnore]
    public string UtpTwoBaths { get; private set; }
    [JsonIgnore]
    public string UtpMasterBedroom { get; private set; }
    [JsonIgnore]
    public string UtpHighflat { get; private set; }
    [JsonIgnore]
    public string UtpBalcony { get; private set; }
    [JsonIgnore]
    public string UtpExtraSummerRooms { get; private set; }
    [JsonIgnore]
    public string UtpTerrace { get; private set; }
    [JsonIgnore]
    public string UtpPantry { get; private set; }
    [JsonIgnore]
    public string UtpLaundry { get; private set; }
    [JsonIgnore]
    public string UtpExtraBalconyArea { get; private set; }

    // Рассчитывает площади квартир на основе актуальных системных площадей помещений
    public void CalculateRevitAreas() {
        _areaMainRevit = _rooms
            .Select(x => x.AreaRevit)
            .Sum();

        _areaCoefRevit = _rooms
            .Select(x => x.AreaCoefRevit)
            .Sum();

        _areaLivingRevit = _rooms
            .Select(x => x.AreaLivingRevit)
            .Sum();

        _areaNonSummerRevit = _rooms
            .Select(x => x.AreaNonSummerRevit)
            .Sum();
    }

    // Проверка актуальности площадей квартир.
    // Сравнивается сумма системных площадей помещений с площадью из квартирографии.
    // Проверка учитывает общую площадь, с коэффициентом, жилую и без летних помещений.
    public bool CheckActualApartmentAreas() {
        if(Math.Abs(AreaMain - _areaMainRevit) > _maxAreaDeviation) {
            return false;
        }
        if(Math.Abs(AreaCoef - _areaCoefRevit) > _maxAreaDeviation) {
            return false;
        }
        return Math.Abs(AreaLiving - _areaLivingRevit) <= _maxAreaDeviation && Math.Abs(AreaNonSummer - _areaNonSummerRevit) <= _maxAreaDeviation;
    }

    // Сравнивает площади квартир, рассчитанные квартирографией.
    // Все помещения в одной квартире должны иметь одинаковые значения.
    public bool CheckEqualityOfRoomAreas() {
        int amountOfAreas = _rooms
            .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracyForArea))
            .Distinct()
            .Count();

        if(amountOfAreas > 1) {
            return false;
        }

        int amountOfAreasCoef = _rooms
            .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaCoefParam, _accuracyForArea))
            .Distinct()
            .Count();

        if(amountOfAreasCoef > 1) {
            return false;
        }

        int amountOfAreasLiving = _rooms
            .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaLivingParam, _accuracyForArea))
            .Distinct()
            .Count();

        if(amountOfAreasLiving > 1) {
            return false;
        }

        int amountOfAreasNonSum = _rooms
            .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaNonSumParam, _accuracyForArea))
            .Distinct()
            .Count();

        return amountOfAreasNonSum <= 1;
    }

    private void AddToDictionary(Dictionary<string, List<RoomElement>> roomDictionary, RoomElement room) {
        if(roomDictionary.ContainsKey(room.Name)) {
            roomDictionary[room.Name].Add(room);
        } else {
            roomDictionary.Add(room.Name, [room]);
        }
    }

    public IReadOnlyList<RoomElement> GetRoomsByPrior(RoomPriority priority) {
        string name = priority.Name;

        return _mainRooms.Keys.Contains(name, _strComparer)
            ? _mainRooms[name]
            : _nonConfigRooms.Keys.Contains(name) ? _nonConfigRooms[name] : (IReadOnlyList<RoomElement>) [];
    }

    public IEnumerable<string> GetOtherPriorityNames() {
        return _nonConfigRooms.Keys;
    }

    public void CalculateUtp(UtpCalculator calculator) {
        UtpTwoBaths = calculator.CalculateTwoBathes(this);
        UtpMasterBedroom = calculator.CalculateMasterBedroom(this);
        UtpHighflat = calculator.CalculateHighflat(this);

        UtpBalcony = calculator.CalculateBalcony(this);
        UtpTerrace = calculator.CalculateTerrace(this);
        UtpExtraSummerRooms = calculator.CalculateExtraSummerRooms(this);
        UtpExtraBalconyArea = calculator.CalculateExtraBalconyArea(this);

        UtpPantry = calculator.CalculatePantry(this);
        UtpLaundry = calculator.CalculateLaundry(this);
    }
}
