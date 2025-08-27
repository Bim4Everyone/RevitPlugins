using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitFinishing.Models.Finishing;
using RevitFinishing.ViewModels.Notices;

namespace RevitFinishing.Services;
internal class FinishingValidationService {
    private readonly ILocalizationService _localizationService;

    public FinishingValidationService(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public IList<WarningElementViewModel> CheckPhaseContainsFinishing(FinishingInProject finishings, 
                                                                      Phase phase) {
        return !finishings.AllFinishing.Any()
            ? [ new WarningElementViewModel(phase, phase.Name, _localizationService) ]
            : [];
    }

    public IList<WarningElementViewModel> CheckFinishingByRoomBounding(FinishingInProject finishings, 
                                                                       Phase phase) {
        return finishings
            .AllFinishing
            .Where(x => x.RevitElement.GetParamValueOrDefault(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0) == 1)
            .Select(x => new WarningElementViewModel(x.RevitElement, phase.Name, _localizationService))
            .ToList();
    }

    public IList<WarningElementViewModel> CheckRoomsByKeyParameter(IEnumerable<Element> rooms,
                                                                   ProjectParam projectParam, 
                                                                   Phase phase) {
        return rooms
            .Where(x => !x.IsExistsParamValue(projectParam))
            .Select(x => new WarningElementViewModel(x, phase.Name, _localizationService))
            .ToList();
    }

    public IList<WarningElementViewModel> CheckRoomsByParameter(IEnumerable<Element> rooms,
                                                                SystemParam systemParam, 
                                                                Phase phase) {
        return rooms
            .Where(x => !x.IsExistsParamValue(systemParam))
            .Select(x => new WarningElementViewModel(x, phase.Name, _localizationService))
            .ToList();
    }

    public IList<WarningElementViewModel> CheckFinishingByRoom(IEnumerable<FinishingElement> finishings, 
                                                               Phase phase) {
        return finishings
            .Where(x => !x.CheckFinishingTypes())
            .Select(x => x.RevitElement)
            .Select(x => new WarningElementViewModel(x, phase.Name, _localizationService))
            .ToList();
    }
}
