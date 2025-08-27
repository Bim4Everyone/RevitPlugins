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
    private readonly Document _document;

    public ProjectValidationService(ILocalizationService localizationService,
                                    RevitRepository revitRepository,
                                    FinishingValidationService finishingValidationService) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _finishingValidationService = finishingValidationService;

        _document = _revitRepository.Document;
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

    public WarningsListViewModel CheckNumberParam(IEnumerable<Room> selectedRooms, Phase phase) {
        var numberParam = SystemParamsConfig.Instance
            .CreateRevitParam(_document, BuiltInParameter.ROOM_NUMBER);

        return new WarningsListViewModel(_localizationService) {
            Description =
                $"{_localizationService.GetLocalizedString("ErrorsWindow.NoParam")} \"{numberParam.Name}\"",
            ErrorElements =
                [.. _finishingValidationService.CheckRoomsByParameter(selectedRooms, numberParam, phase)]
        };
    }

    public WarningsListViewModel CheckNameParam(IEnumerable<Room> selectedRooms, Phase phase) {
        var nameParam = SystemParamsConfig.Instance
            .CreateRevitParam(_document, BuiltInParameter.ROOM_NUMBER);

        return new WarningsListViewModel(_localizationService) {
            Description =
                $"{_localizationService.GetLocalizedString("ErrorsWindow.NoParam")} \"{nameParam.Name}\"",
            ErrorElements = 
                [.. _finishingValidationService.CheckRoomsByParameter(selectedRooms, nameParam, phase)]
        };
    }

    public WarningsListViewModel CheckCustomFamilies(IEnumerable<FinishingElement> finishingElements,
                                                     Phase phase) {
        return new WarningsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.CustomFamilies"),
            ErrorElements = [.. finishingElements
                .Where(x => x.IsCustomFamily)
                .Select(x => new WarningElementViewModel(x.RevitElement, phase.Name, _localizationService))]
        };
    }

    public WarningsListViewModel CheckUnusedFinishing(FinishingCalculator calculator,
                                                   Phase phase) {
        WarningsListViewModel warningList = new WarningsListViewModel(_localizationService) {
            Description = _localizationService.GetLocalizedString("ErrorsWindow.UnusedFinishingTypes"),
            ErrorElements = []
        };

        var roomsByFinishingType = calculator.RoomsByFinishingType;
        IList<KeyFinishingType> keys = _revitRepository.GetKeyFinishingTypes();
        foreach(var keyFinishingType in keys) {
            if(roomsByFinishingType.TryGetValue(keyFinishingType.Name, out FinishingType finishingType)) {
                if(finishingType.WallTypesNumber != keyFinishingType.NumberOfWalls) {
                    string categoryName = _localizationService.GetLocalizedString("ErrorsWindow.Walls");
                    warningList.ErrorElements.Add(
                        new WarningScheduleKeyVM(
                            keyFinishingType.Element, phase.Name, categoryName, _localizationService));                
                }
                if(finishingType.FloorTypesNumber != keyFinishingType.NumberOfFloors) {
                    string categoryName = _localizationService.GetLocalizedString("ErrorsWindow.Floors");
                    warningList.ErrorElements.Add(
                        new WarningScheduleKeyVM(
                            keyFinishingType.Element, phase.Name, categoryName, _localizationService));
                }
                if(finishingType.BaseboardTypesNumber != keyFinishingType.NumberOfBaseboards) {
                    string categoryName = _localizationService.GetLocalizedString("ErrorsWindow.Baseboards");
                    warningList.ErrorElements.Add(
                        new WarningScheduleKeyVM(
                            keyFinishingType.Element, phase.Name, categoryName, _localizationService));
                }
                if(finishingType.CeilingTypesNumber != keyFinishingType.NumberOfCeilings) {
                    string categoryName = _localizationService.GetLocalizedString("ErrorsWindow.Ceilings");
                    warningList.ErrorElements.Add(
                        new WarningScheduleKeyVM(
                            keyFinishingType.Element, phase.Name, categoryName, _localizationService));
                }
            }
        }

        return warningList;
    }
}
