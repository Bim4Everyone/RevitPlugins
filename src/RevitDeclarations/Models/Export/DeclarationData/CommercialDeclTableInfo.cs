using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models.Export.DeclarationData {
    internal class CommercialDeclTableInfo : ITableInfo {
        private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;
        private readonly DeclarationSettings _settings;

        public CommercialDeclTableInfo(IReadOnlyCollection<CommercialRooms> commercialRooms, DeclarationSettings settings) {
            _commercialRooms = commercialRooms;
            _settings = settings;
        }
    }
}
