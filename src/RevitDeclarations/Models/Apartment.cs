using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class Apartment {
        private const double _maxAreaDeviation = 0.2;

        private readonly DeclarationSettings _settings;
        private readonly PrioritiesConfig _priorConfig;
        private readonly int _accuracy;

        private readonly IReadOnlyCollection<RoomElement> _rooms;
        // Словарь для группировки помещений, входящих в исходные приоритеты
        private readonly Dictionary<string, List<RoomElement>> _mainRooms;
        // Словарь для группировки помещений, не входящих в исходные приоритеты
        private readonly Dictionary<string, List<RoomElement>> _otherRooms;
        private readonly RoomElement _firstRoom;

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

        public Apartment(IEnumerable<RoomElement> rooms, DeclarationSettings settings) {
            _settings = settings;
            _accuracy = settings.Accuracy;
            _priorConfig = _settings.PrioritiesConfig;

            _rooms = rooms.ToList();
            _firstRoom = rooms.FirstOrDefault();

            _mainRooms = new Dictionary<string, List<RoomElement>>();
            _otherRooms = new Dictionary<string, List<RoomElement>>();

            foreach(RoomElement room in _rooms) {
                if(settings.MainRoomNames.Contains(room.NameLower)) {
                    AddToDictionary(_mainRooms, room);
                } else {
                    AddToDictionary(_otherRooms, room);
                }
            }

            CalculateRevitAreas();
        }

        [JsonProperty("rooms")]
        public IReadOnlyCollection<RoomElement> Rooms => _rooms;

        [JsonProperty("full_number")]
        public string FullNumber => _firstRoom.GetTextParamValue(_settings.ApartmentFullNumberParam);
        [JsonProperty("type")]
        public string Department {
            get {
                if(string.IsNullOrEmpty(_firstRoom.GetTextParamValue(_settings.MultiStoreyParam))) {
                    return _firstRoom.GetTextParamValue(_settings.DepartmentParam);
                } else {
                    return "Квартира на двух и более этажах";
                }
            }
        }
        [JsonProperty("floor_number")]
        public string Level { 
            get {
                var levelNames = _rooms
                    .Select(x => x.GetTextParamValue(_settings.LevelParam))
                    .Distinct();
                return string.Join(",", levelNames); 
            } 
        }
        [JsonProperty("section")]
        public string Section => _firstRoom.GetTextParamValue(_settings.SectionParam);
        [JsonProperty("building")]
        public string Building => _firstRoom.GetTextParamValue(_settings.BuildingParam);
        [JsonProperty("number")]
        public string Number => _firstRoom.GetTextParamValue(_settings.ApartmentNumberParam);
        [JsonProperty("area")]
        public double AreaMain => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracy);
        [JsonProperty("area_k")]
        public double AreaCoef => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaCoefParam, _accuracy);
        [JsonProperty("area_living")]
        public double AreaLiving => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaLivingParam, _accuracy);
        [JsonProperty("area_non_summer")]
        public double AreaNonSummer => _firstRoom.GetAreaParamValue(_settings.ApartmentAreaNonSumParam, _accuracy);
        [JsonProperty("room_size")]
        public int RoomsAmount => _firstRoom.GetIntParamValue(_settings.RoomsAmountParam);
        [JsonProperty("ceiling_height")]
        public double RoomsHeight => _firstRoom.GetLengthParamValue(_settings.RoomsHeightParam, _accuracy);

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

        // Calculates apartment areas based on the current system room area property
        public void CalculateRevitAreas() {
            _areaMainRevit = _rooms
                .Select(x => x.AreaRevit)
                .Sum();

            _areaCoefRevit = _rooms
                .Select(x => x.AreaCoefRevit)
                .Sum();

            _areaLivingRevit = _rooms
                .Where(x => _priorConfig.LivingRoom.CheckName(x.Name))
                .Select(x => x.AreaRevit)
                .Sum();

            _areaNonSummerRevit = _rooms
                .Where(x => x.AreaRevit == x.AreaCoefRevit)
                .Select(x => x.AreaRevit)
                .Sum();
        }

        public bool CheckActualRoomAreas() {
            foreach(var room in _rooms) {
                if(Math.Abs(room.AreaRevit - room.Area) > _maxAreaDeviation) {
                    return false;
                }

                if(Math.Abs(room.AreaCoefRevit - room.AreaCoef) > _maxAreaDeviation) {
                    return false;
                }
            }

            return true;
        }

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

        // This method compares the areas calculated by rooms script.
        // Every room in the apartment should have the same area value.
        public bool CheckEqualityOfRoomAreas() {
            int amountOfAreas = _rooms
                .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaParam, _accuracy))
                .Distinct()
                .Count();
            if(amountOfAreas > 1) {
                return false;
            }

            int amountOfAreasCoef = _rooms
                .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaCoefParam, _accuracy))
                .Distinct()
                .Count();

            if(amountOfAreasCoef > 1) {
                return false;
            }

            int amountOfAreasLiving = _rooms
                .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaLivingParam, _accuracy))
                .Distinct()
                .Count();

            if(amountOfAreasLiving > 1) {
                return false;
            }

            int amountOfAreasNonSum = _rooms
                .Select(x => x.GetAreaParamValue(_settings.ApartmentAreaNonSumParam, _accuracy))
                .Distinct()
                .Count();

            if(amountOfAreasNonSum > 1) {
                return false;
            }

            return true;
        }

        private void AddToDictionary(Dictionary<string, List<RoomElement>> roomDictionary, RoomElement room) {
            if(roomDictionary.ContainsKey(room.NameLower)) {
                roomDictionary[room.NameLower].Add(room);
            } else {
                roomDictionary.Add(room.NameLower, new List<RoomElement> { room });
            }
        }

        public IReadOnlyCollection<RoomElement> GetRoomsByPrior(RoomPriority priority) {
            string name = priority.NameLower;

            if(_mainRooms.Keys.Contains(name)) {
                return _mainRooms[name];
            } else if(_otherRooms.Keys.Contains(name)) {
                return _otherRooms[name];
            } else {
                return new List<RoomElement>();
            }
        }

        public IReadOnlyCollection<string> GetOtherPriorityNames() {
            return _otherRooms.Keys;
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
