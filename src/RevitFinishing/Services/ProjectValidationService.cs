using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.SimpleServices;

using RevitFinishing.Models;
using RevitFinishing.Models.Finishing;
using RevitFinishing.ViewModels.Notices;

namespace RevitFinishing.Services;
internal class ProjectValidationService {
    private readonly FinishingValidationService _finishingValidationService;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public ProjectValidationService(ILocalizationService localizationService,
                                    RevitRepository revitRepository,
                                    FinishingValidationService finishingValidationService) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
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
                                           Phase phase) {
        Document document = _revitRepository.Document;
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





        WarningsListViewModel warningList = new WarningsListViewModel(_localizationService) {
            Description = "Unused finishing types!"
        };

        var roomsByFinishingType = calculator.RoomsByFinishingType;
        IList<KeyFinishingType> keys = _revitRepository.GetKeyFinishingTypes();
        foreach(var keyFinishingType in keys) {
            var finishingType = roomsByFinishingType[keyFinishingType.Name];

            if(finishingType.WallTypesNumber != keyFinishingType.NumberOfWalls) {
                warningList.ErrorElements.Add(new NoticeElementViewModel(keyFinishingType, phase.Name, _localizationService));

                
            }
        }


        parameterErrors.AddElements(warningList);

        return parameterErrors;
    }
}
