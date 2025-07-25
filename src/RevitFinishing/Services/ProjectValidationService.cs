using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.SimpleServices;

using RevitFinishing.Models.Finishing;
using RevitFinishing.ViewModels.Notices;

namespace RevitFinishing.Services;
internal class ProjectValidationService {
    private readonly FinishingValidationService _finishingValidationService;
    private readonly ILocalizationService _localizationService;

    public ProjectValidationService(ILocalizationService localizationService,
                                    FinishingValidationService finishingValidationService) {
        _localizationService = localizationService;
        _finishingValidationService = finishingValidationService;
    }

    public ErrorsViewModel CheckMainErrors(FinishingInProject allFinishing,
                                            IEnumerable<Room> selectedRooms,
                                            Phase phase) {
        var mainErrors = new ErrorsViewModel(_localizationService);

        mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.NoFinishing"),
            ErrorElements = [.. _finishingValidationService.CheckPhaseContainsFinishing(allFinishing, phase)]
        });
        mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.NoBoundaries"),
            ErrorElements = [.. _finishingValidationService.CheckFinishingByRoomBounding(allFinishing, phase)]
        });
        var finishingKeyParam = ProjectParamsConfig.Instance.RoomFinishingType;
        mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.NoKeyParam"),
            ErrorElements = 
                [.. _finishingValidationService.CheckRoomsByKeyParameter(selectedRooms, finishingKeyParam, phase)]
        });

        return mainErrors;
    }

    public ErrorsViewModel CheckFinishingErrors(FinishingCalculator calculator, Phase phase) {
        var finishingErrors = new ErrorsViewModel(_localizationService);

        finishingErrors.AddElements(new ErrorsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.DifPhases"),
            ErrorElements = 
                [.. _finishingValidationService.CheckFinishingByRoom(calculator.FinishingElements, phase)]
        });

        return finishingErrors;
    }

    public WarningsViewModel CheckWarnings(IEnumerable<Room> selectedRooms,
                                           IEnumerable<FinishingElement> finishingElements,
                                           Phase phase,
                                           Document document) {
        var parameterErrors = new WarningsViewModel(_localizationService);

        var numberParam = SystemParamsConfig.Instance.CreateRevitParam(document, BuiltInParameter.ROOM_NUMBER);
        parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
            Description =
                $"{_localizationService.GetLocalizedString("ErrorsWindow.NoParam")} \"{numberParam.Name}\"",
            ErrorElements = 
                [.. _finishingValidationService.CheckRoomsByParameter(selectedRooms, numberParam, phase)]
        });

        var nameParam = SystemParamsConfig.Instance.CreateRevitParam(document, BuiltInParameter.ROOM_NUMBER);
        parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
            Description =
                $"{_localizationService.GetLocalizedString("ErrorsWindow.NoParam")} \"{nameParam.Name}\"",
            ErrorElements = 
                [.. _finishingValidationService.CheckRoomsByParameter(selectedRooms, nameParam, phase)]
        });

        parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.CustomFamilies"),
            ErrorElements = [.. finishingElements
                .Where(x => x.IsCustomFamily)
                .Select(x => new NoticeElementViewModel(x.RevitElement, phase.Name, _localizationService))]
        });

        return parameterErrors;
    }
}
