using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class ApartmentsTableInfo : ITableInfo {
        public const int MainRoomCells = 3;
        public const int SummerRoomCells = 4;

        public readonly int UtpWidth = 9;

        private readonly IReadOnlyCollection<Apartment> _apartments;
        private readonly DeclarationSettings _settings;
        private readonly int _roomGroupsInfoWidth;

        private int _fullTableWidth;
        private int _summerRoomsStart;
        private int _otherRoomsStart;
        private int _utpStart;
        private readonly int[] _columnsWithDoubleType;

        public ApartmentsTableInfo(IReadOnlyCollection<Apartment> apartments, DeclarationSettings settings) {
            _apartments = apartments;
            _settings = settings;

            _roomGroupsInfoWidth = 15;
            if(!_settings.LoadUtp) {
                UtpWidth = 0;
            }

            List<string> otherNames = GetOtherPriorities();
            settings.UpdatePriorities(otherNames);

            CountRoomsForPriorities();
            CalculateTableSizes();

            int[] mainColumnsIndexes = new int[] { 7, 8, 9, 10, 13, 14 };
            _columnsWithDoubleType = mainColumnsIndexes
                .Concat(FindDataColumns())
                .ToArray();
        }

        public IReadOnlyCollection<RoomGroup> RoomGroups => _apartments;
        public int[] ColumnsWithDoubleType => _columnsWithDoubleType;
        public DeclarationSettings Settings => _settings;
        public int FullTableWidth => _fullTableWidth;
        public int RoomGroupsInfoWidth => _roomGroupsInfoWidth;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;

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

            _fullTableWidth = RoomGroupsInfoWidth
                + mainRoomsWidth
                + summerRoomsWidth
                + otherRoomsWidth
                + UtpWidth;

            _summerRoomsStart = RoomGroupsInfoWidth + mainRoomsWidth;
            _otherRoomsStart = RoomGroupsInfoWidth + mainRoomsWidth + summerRoomsWidth;
            _utpStart = RoomGroupsInfoWidth + mainRoomsWidth + summerRoomsWidth + otherRoomsWidth;
        }

        private int[] FindDataColumns() {
            List<int> columnsIndexes = new List<int>();
            int columnNumber = RoomGroupsInfoWidth;

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
