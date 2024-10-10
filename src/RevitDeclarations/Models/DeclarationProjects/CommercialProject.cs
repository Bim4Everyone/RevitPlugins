using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class CommercialProject : DeclarationProject {
        public CommercialProject(RevitDocumentViewModel document,
                                RevitRepository revitRepository,
                                DeclarationSettings settings) 
            : base(document, revitRepository, settings) {
            _roomGroups = revitRepository.GetCommercialRooms(_rooms, settings);
        }
    }
}
