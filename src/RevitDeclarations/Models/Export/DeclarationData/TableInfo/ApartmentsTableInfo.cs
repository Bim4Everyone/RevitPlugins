using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class ApartmentsTableInfo : ITableInfo {
        public const int MainRoomCells = 3;
        public const int SummerRoomCells = 4;

        private readonly int _rowsTotalNumber;
        private readonly int _groupsInfoColumnsNumber;
        private readonly int[] _numericColumnsIndexes;
        private readonly int[] _areaTypeColumnsIndexes;
        private readonly int[] _lengthTypeColumnsIndexes;
        private readonly int _utpWidth = 9;

        private readonly DeclarationSettings _settings;
        private readonly IReadOnlyCollection<Apartment> _apartments;

        private int _columnsTotalNumber;
        private int _summerRoomsStart;
        private int _otherRoomsStart;
        private int _utpStart;

        public ApartmentsTableInfo(IReadOnlyCollection<Apartment> apartments, DeclarationSettings settings) {
            _apartments = apartments;
            _settings = settings;

            _groupsInfoColumnsNumber = 15;
            if(!_settings.LoadUtp) {
                _utpWidth = 0;
            }

            _rowsTotalNumber = RoomGroups.Count;

            List<string> otherNames = GetOtherPriorities();
            settings.UpdatePriorities(otherNames);

            CountRoomsForPriorities();
            CalculateTableSizes();

            int[] mainColumnsNumIndexes = new int[] { 7, 8, 9, 13, 14 };
            _areaTypeColumnsIndexes = mainColumnsNumIndexes
                .Concat(FindNumericColumns())
                .ToArray();
            _lengthTypeColumnsIndexes = new int[] { 10 };

            _numericColumnsIndexes = _areaTypeColumnsIndexes
                .Concat(_lengthTypeColumnsIndexes)
                .ToArray();
        }

        public int ColumnsTotalNumber => _columnsTotalNumber;
        public int RowsTotalNumber => _rowsTotalNumber;

        public int GroupsInfoColumnsNumber => _groupsInfoColumnsNumber;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
        public int[] NumericColumnsIndexes => _numericColumnsIndexes;
        public int[] AreaTypeColumnsIndexes => _areaTypeColumnsIndexes;
        public int[] LengthTypeColumnsIndexes => _lengthTypeColumnsIndexes;

        public DeclarationSettings Settings => _settings;
        public IReadOnlyCollection<RoomGroup> RoomGroups => _apartments;

        private List<string> GetOtherPriorities() {
            return _apartments
               .SelectMany(x => x.GetOtherPriorityNames())
               .Distinct()
               .ToList();
        }

        private void CountRoomsForPriorities() {
            foreach(RoomPriority priority in _settings.Priorities) {
                int numberOfRooms = _apartments
                    .Select(x => x.GetRoomsByPrior(priority).Count)
                    .Max();

                priority.MaxRoomAmount = numberOfRooms;
            }
        }

        private void CalculateTableSizes() {
            int mainRoomsWidth = _settings
                .UsedPriorities
                .Where(x => !x.IsSummer)
                .Where(x => !x.IsNonConfig)
                .Select(x => x.MaxRoomAmount * MainRoomCells)
                .Sum();

            int summerRoomsWidth = _settings
                .UsedPriorities
                .Where(x => x.IsSummer)
                .Where(x => !x.IsNonConfig)
                .Select(x => x.MaxRoomAmount * SummerRoomCells)
                .Sum();

            int otherRoomsWidth = _settings
                .UsedPriorities
                .Where(x => !x.IsSummer)
                .Where(x => x.IsNonConfig)
                .Select(x => x.MaxRoomAmount * MainRoomCells)
                .Sum();

            _columnsTotalNumber = GroupsInfoColumnsNumber
                + mainRoomsWidth
                + summerRoomsWidth
                + otherRoomsWidth
                + _utpWidth;

            _summerRoomsStart = GroupsInfoColumnsNumber + mainRoomsWidth;
            _otherRoomsStart = GroupsInfoColumnsNumber + mainRoomsWidth + summerRoomsWidth;
            _utpStart = GroupsInfoColumnsNumber + mainRoomsWidth + summerRoomsWidth + otherRoomsWidth;
        }

        private int[] FindNumericColumns() {
            List<int> columnsIndexes = new List<int>();
            int columnNumber = GroupsInfoColumnsNumber;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * SummerRoomCells;
                        columnsIndexes.Add(columnIndex + 2);
                        columnsIndexes.Add(columnIndex + 3);
                    }
                    columnNumber += priority.MaxRoomAmount * SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * MainRoomCells;
                        columnsIndexes.Add(columnIndex + 2);
                    }
                    columnNumber += priority.MaxRoomAmount * MainRoomCells;
                }
            }
            return columnsIndexes.ToArray();
        }
    }
}
