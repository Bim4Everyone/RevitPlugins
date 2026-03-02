using System.Linq;

using dosymep.Revit.Comparators;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class CommercialProject : DeclarationProject {
    public CommercialProject(RevitDocumentViewModel document,
                             RevitRepository revitRepository,
                             DeclarationSettings settings, 
                             LogicalStringComparer logicalStrComparer)
        : base(document, revitRepository, settings, logicalStrComparer) {
        var paramProvider = new RoomParamProvider(settings);
        _roomGroups = revitRepository
            .GroupRooms(_rooms, settings)
            .Select(x => new CommercialRooms(x, settings, paramProvider, logicalStrComparer))
            .ToList();
    }
}
