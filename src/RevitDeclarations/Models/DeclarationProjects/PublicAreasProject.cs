using System.Linq;

using dosymep.Revit.Comparators;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class PublicAreasProject : DeclarationProject {
    public PublicAreasProject(RevitDocumentViewModel document,
                              RevitRepository revitRepository,
                              DeclarationSettings settings, 
                              LogicalStringComparer logicalStrComparer)
        : base(document, revitRepository, settings, logicalStrComparer) {
        var paramProvider = new RoomParamProvider(settings);
        _roomGroups = revitRepository
            .GroupRooms(_rooms, settings)
            .Select(x => new PublicArea(x, settings, paramProvider, logicalStrComparer))
            .ToList();
    }
}
