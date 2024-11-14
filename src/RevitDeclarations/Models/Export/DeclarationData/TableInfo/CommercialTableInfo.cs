using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialTableInfo : ITableInfo {
        private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;
        private readonly DeclarationSettings _settings;

        private readonly int _fullTableWidth;
        private readonly int _summerRoomsStart;
        private readonly int _otherRoomsStart;
        private readonly int _utpStart;
        private readonly int[] _columnsWithDoubleType;
        private readonly int _rowsNumber;

        public CommercialTableInfo(IReadOnlyCollection<CommercialRooms> commercialRooms, 
                                   DeclarationSettings settings) {
            _commercialRooms = commercialRooms;
            _settings = settings;

            _fullTableWidth = 13;
            _summerRoomsStart = 0;
            _otherRoomsStart = 0;
            _utpStart = 0;
            _columnsWithDoubleType = new int[] { 7, 8 };
            _rowsNumber = RoomGroups.Count;
        }

        public IReadOnlyCollection<RoomGroup> RoomGroups => _commercialRooms;
        public int[] ColumnsWithDoubleType => _columnsWithDoubleType;
        public DeclarationSettings Settings => _settings;
        public int FullTableWidth => _fullTableWidth;
        public int RowsNumber => _rowsNumber;
        public int RoomGroupsInfoWidth => _fullTableWidth;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
    }
}
