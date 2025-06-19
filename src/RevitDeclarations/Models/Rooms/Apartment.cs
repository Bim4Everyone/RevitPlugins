using System;
using System.Collections.Generic;
using System.Linq;
using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
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

        private string _utpTwoBaths;
        private string _utpMasterBedroom;
        private string _utpHighflat;
        private string _utpBalcony;
        private string _utpExtraSummerRooms;
        private string _utpTerrace;
        private string _utpPantry;
        private string _utpLaundry;
        private string _utpExtraBalconyArea;

        public Apartment(IEnumerable<RoomElement> rooms, 
                         DeclarationSettings settings, 
                         RoomParamProvider paramProvider) 
            : base(rooms, settings, paramProvider) {
            _settings = (ApartmentsSettings) settings;
            _mainRooms = new Dictionary<string, List<RoomElement>>(_strComparer);
            _nonConfigRooms = new Dictionary<string, List<RoomElement>>(_strComparer);

            foreach(RoomElement room in _rooms) {
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
        public string UtpTwoBaths => _utpTwoBaths;
        [JsonIgnore]
        public string UtpMasterBedroom => _utpMasterBedroom;
        [JsonIgnore]
        public string UtpHighflat => _utpHighflat;
        [JsonIgnore]
        public string UtpBalcony => _utpBalcony;
        [JsonIgnore]
        public string UtpExtraSummerRooms => _utpExtraSummerRooms;
        [JsonIgnore]
        public string UtpTerrace => _utpTerrace;
        [JsonIgnore]
        public string UtpPantry => _utpPantry;
        [JsonIgnore]
        public string UtpLaundry => _utpLaundry;
        [JsonIgnore]
        public string UtpExtraBalconyArea => _utpExtraBalconyArea;

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
            if(Math.Abs(AreaLiving - _areaLivingRevit) > _maxAreaDeviation) {
                return false;
            }
            if(Math.Abs(AreaNonSummer - _areaNonSummerRevit) > _maxAreaDeviation) {
                return false;
            }

            return true;
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

            if(amountOfAreasNonSum > 1) {
                return false;
            }

            return true;
        }

        private void AddToDictionary(Dictionary<string, List<RoomElement>> roomDictionary, RoomElement room) {
            if(roomDictionary.ContainsKey(room.Name)) {
                roomDictionary[room.Name].Add(room);
            } else {
                roomDictionary.Add(room.Name, new List<RoomElement> { room });
            }
        }

        public IReadOnlyList<RoomElement> GetRoomsByPrior(RoomPriority priority) {
            string name = priority.Name;

            if(_mainRooms.Keys.Contains(name, _strComparer)) {
                return _mainRooms[name];
            } else if(_nonConfigRooms.Keys.Contains(name)) {
                return _nonConfigRooms[name];
            } else {
                return new List<RoomElement>();
            }
        }

        public IEnumerable<string> GetOtherPriorityNames() {
            return _nonConfigRooms.Keys;
        }

        public void CalculateUtp(UtpCalculator calculator) {
            _utpTwoBaths = calculator.CalculateTwoBathes(this);
            _utpMasterBedroom = calculator.CalculateMasterBedroom(this);
            _utpHighflat = calculator.CalculateHighflat(this);

            _utpBalcony = calculator.CalculateBalcony(this);
            _utpTerrace = calculator.CalculateTerrace(this);
            _utpExtraSummerRooms = calculator.CalculateExtraSummerRooms(this);
            _utpExtraBalconyArea = calculator.CalculateExtraBalconyArea(this);

            _utpPantry = calculator.CalculatePantry(this);
            _utpLaundry = calculator.CalculateLaundry(this);
        }
    }
}
