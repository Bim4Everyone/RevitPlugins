using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class CommercialGroupTableInfo : ITableInfo {
        private readonly int _columnsTotalNumber;
        private readonly int _rowsTotalNumber;
        private readonly int _summerRoomsStart;
        private readonly int _otherRoomsStart;
        private readonly int _utpStart;
        private readonly int[] _numericColumnsIndexes;

        private readonly DeclarationSettings _settings;
        private readonly IReadOnlyCollection<CommercialRooms> _roomGroups;

        public CommercialGroupTableInfo(IReadOnlyCollection<CommercialRooms> roomGroups,
                                        DeclarationSettings settings) {
            _roomGroups = roomGroups;
            _settings = settings;

            _columnsTotalNumber = 3;
            _summerRoomsStart = 0;
            _otherRoomsStart = 0;
            _utpStart = 0;
            _numericColumnsIndexes = new int[] { 1 };
            _rowsTotalNumber = RoomGroups.First().Rooms.Count();
        }

        public int ColumnsTotalNumber => _columnsTotalNumber;
        public int RowsTotalNumber => _rowsTotalNumber;

        public int GroupsInfoColumnsNumber => _columnsTotalNumber;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
        public int[] NumericColumnsIndexes => _numericColumnsIndexes;

        public DeclarationSettings Settings => _settings;
        public IReadOnlyCollection<RoomGroup> RoomGroups => _roomGroups;
    }
}
