using System.Linq;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class PublicAreasProject : DeclarationProject {
    public PublicAreasProject(RevitDocumentViewModel document,
                              RevitRepository revitRepository,
                              DeclarationSettings settings)
        : base(document, revitRepository, settings) {
        var paramProvider = new RoomParamProvider(settings);
        _roomGroups = revitRepository
            .GroupRooms(_rooms, settings)
            .Select(x => new PublicArea(x, settings, paramProvider))
            .ToList();
    }
}
