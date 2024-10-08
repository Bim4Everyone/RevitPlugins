using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models.Export.DeclarationData {
    internal class CommercialDeclTableInfo : ITableInfo {
        private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;
        private readonly DeclarationSettings _settings;

        private readonly int _fullTableWidth;

        public CommercialDeclTableInfo(IReadOnlyCollection<CommercialRooms> commercialRooms, 
                                       DeclarationSettings settings) {
            _commercialRooms = commercialRooms;
            _settings = settings;

            _fullTableWidth = 11;
        }

        public IReadOnlyCollection<RoomGroup> RoomGroups => _commercialRooms;
        public int FullTableWidth => _fullTableWidth;

    }
}
