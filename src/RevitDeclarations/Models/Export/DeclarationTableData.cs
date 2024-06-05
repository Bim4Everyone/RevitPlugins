using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class DeclarationTableData {
        public readonly int MainRoomCells = 3;
        public readonly int SummerRoomCells = 4;

        public readonly int InfoWidth = 13;
        public readonly int UtpWidth = 9;

        private readonly List<Apartment> _apartments;
        private readonly DeclarationSettings _settings;

        private int _fullTableWidth;
        private int _summerRoomsStart;
        private int _otherRoomsStart;
        private int _utpStart;

        public DeclarationTableData(List<Apartment> apartments, DeclarationSettings settings) {
            _apartments = apartments;
            _settings = settings;

            if(!_settings.LoadUtp) {
                UtpWidth = 0;
            }

            List<string> otherNames = GetOtherPriorities();
            settings.UpdatePriorities(otherNames);

            CountRoomsForPriorities();
            CalculateTableSizes();
        }

        public List<Apartment> Apartments => _apartments;

        public int FullTableWidth => _fullTableWidth;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;

        private List<string> GetOtherPriorities() {
            return _apartments
               .SelectMany(x => x.OtherRooms.Keys)
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
                .Where(x => !x.IsOther)
                .Select(x => x.MaxRoomAmount * MainRoomCells)
                .Sum();

            int summerRoomsWidth = _settings
                .UsedPriorities
                .Where(x => x.IsSummer)
                .Where(x => !x.IsOther)
                .Select(x => x.MaxRoomAmount * SummerRoomCells)
                .Sum();

            int otherRoomsWidth = _settings
                .UsedPriorities
                .Where(x => !x.IsSummer)
                .Where(x => x.IsOther)
                .Select(x => x.MaxRoomAmount * MainRoomCells)
                .Sum();

            _fullTableWidth = InfoWidth
                + mainRoomsWidth
                + summerRoomsWidth
                + otherRoomsWidth
                + UtpWidth;

            _summerRoomsStart = InfoWidth + mainRoomsWidth;
            _otherRoomsStart = InfoWidth + mainRoomsWidth + summerRoomsWidth;
            _utpStart = InfoWidth + mainRoomsWidth + summerRoomsWidth + otherRoomsWidth;
        }
    }
}
