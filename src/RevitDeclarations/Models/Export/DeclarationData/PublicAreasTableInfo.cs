using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models
{
    internal class PublicAreasTableInfo : ITableInfo {
        private readonly IReadOnlyCollection<PublicArea> _publicAreas;
        private readonly DeclarationSettings _settings;

        private readonly int _fullTableWidth;
        private readonly int _summerRoomsStart;
        private readonly int _otherRoomsStart;
        private readonly int _utpStart;

        public PublicAreasTableInfo(IReadOnlyCollection<PublicArea> publicAreas,
                                    DeclarationSettings settings) {
            _publicAreas = publicAreas;
            _settings = settings;

            _fullTableWidth = 6;
            _summerRoomsStart = 0;
            _otherRoomsStart = 0;
            _utpStart = 0;
        }

        public IReadOnlyCollection<RoomGroup> RoomGroups => _publicAreas;
        public int FullTableWidth => _fullTableWidth;
        public int SummerRoomsStart => _summerRoomsStart;
        public int OtherRoomsStart => _otherRoomsStart;
        public int UtpStart => _utpStart;
    }
}
