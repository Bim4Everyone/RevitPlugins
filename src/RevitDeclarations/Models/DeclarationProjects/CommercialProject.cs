using System.Linq;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models;
internal class CommercialProject : DeclarationProject {
    public CommercialProject(RevitDocumentViewModel document,
                            RevitRepository revitRepository,
                            DeclarationSettings settings,
                            ILocalizationService localizationService)
        : base(document, revitRepository, settings, localizationService) {
        var paramProvider = new RoomParamProvider(settings);
        _roomGroups = revitRepository
            .GroupRooms(_rooms, settings)
            .Select(x => new CommercialRooms(x, settings, paramProvider))
            .ToList();
    }
}
