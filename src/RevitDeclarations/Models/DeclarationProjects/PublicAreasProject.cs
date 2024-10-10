using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models
{
    internal class PublicAreasProject : DeclarationProject {
        public PublicAreasProject(RevitDocumentViewModel document,
                                  RevitRepository revitRepository,
                                  DeclarationSettings settings)
            : base(document, revitRepository, settings) {
            _roomGroups = revitRepository.GetPublicAreas(_rooms, settings);
        }
    }
}
