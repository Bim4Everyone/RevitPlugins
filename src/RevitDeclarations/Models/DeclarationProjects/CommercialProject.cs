using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class CommercialProject : DeclarationProject {
        private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;

        public CommercialProject(RevitDocumentViewModel document,
                                RevitRepository revitRepository,
                                DeclarationSettings settings) 
            : base(document, revitRepository, settings) {

            _commercialRooms = revitRepository.GetCommercialRooms(_rooms, settings);
        }

        public IReadOnlyCollection<CommercialRooms> CommercialRooms => _commercialRooms;
    }
}
