using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialTableInfo : ITableInfo {
        private readonly int _columnsTotalNumber;
        private readonly int _rowsTotalNumber;
        private readonly int _summerRoomsStart;
        private readonly int _otherRoomsStart;
        private readonly int _utpStart;
        private readonly int[] _numericColumnsIndexes;
        private readonly int[] _areaTypeColumnsIndexes;
        private readonly int[] _lengthTypeColumnsIndexes;

        private readonly DeclarationSettings _settings;
        private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;

        public CommercialTableInfo(IReadOnlyCollection<CommercialRooms> commercialRooms, 
                                   DeclarationSettings settings) {
            _commercialRooms = commercialRooms;
            _settings = settings;

            _columnsTotalNumber = 13;
            _summerRoomsStart = 0;
            _otherRoomsStart = 0;
            _utpStart = 0;
            _numericColumnsIndexes = new int[] { 7, 8 };
            _rowsTotalNumber = RoomGroups.Count;

            _areaTypeColumnsIndexes = new int[] { 7 };
            _lengthTypeColumnsIndexes = new int[] { 8 };
            _numericColumnsIndexes = _areaTypeColumnsIndexes
                .Concat(_lengthTypeColumnsIndexes)
                .ToArray();
        }

        public int ColumnsTotalNumber => _columnsTotalNumber;
        public int RowsTotalNumber => _rowsTotalNumber;
        public int GroupsInfoColumnsNumber => _columnsTotalNumber;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
        public int[] NumericColumnsIndexes => _numericColumnsIndexes;
        public int[] AreaTypeColumnsIndexes => _areaTypeColumnsIndexes;
        public int[] LengthTypeColumnsIndexes => _lengthTypeColumnsIndexes;

        public DeclarationSettings Settings => _settings;
        public IReadOnlyCollection<RoomGroup> RoomGroups => _commercialRooms;
    }
}
