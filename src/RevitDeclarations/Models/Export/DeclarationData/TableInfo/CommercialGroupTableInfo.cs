using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialGroupTableInfo : ITableInfo {
        private readonly IReadOnlyCollection<CommercialRooms> _roomGroups;
        private readonly DeclarationSettings _settings;

        private readonly int _fullTableWidth;
        private readonly int _summerRoomsStart;
        private readonly int _otherRoomsStart;
        private readonly int _utpStart;

        public CommercialGroupTableInfo(IReadOnlyCollection<CommercialRooms> roomGroups,
                                        DeclarationSettings settings) {
            _roomGroups = roomGroups;
            _settings = settings;

            _fullTableWidth = 3;
            _summerRoomsStart = 0;
            _otherRoomsStart = 0;
            _utpStart = 0;
        }

        public IReadOnlyCollection<RoomGroup> RoomGroups => _roomGroups;
        public int FullTableWidth => _fullTableWidth;
        public int RoomGroupsInfoWidth => _fullTableWidth;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
    }
}
