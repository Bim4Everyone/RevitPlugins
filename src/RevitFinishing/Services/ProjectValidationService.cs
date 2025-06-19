using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.SimpleServices;

using RevitFinishing.Models.Finishing;
using RevitFinishing.ViewModels;
using RevitFinishing.ViewModels.Errors;

namespace RevitFinishing.Services
{
    internal class ProjectValidationService {
        private readonly FinishingChecker _finishingChecker;
        private readonly ILocalizationService _localizationService;

        public ProjectValidationService(ILocalizationService localizationService,
                                        FinishingChecker finishingChecker) {
            _localizationService = localizationService;
            _finishingChecker = finishingChecker;
        }

        public ErrorsViewModel CheckMainErrors(FinishingInProject allFinishing, 
                                               List<Room> selectedRooms,
                                               Phase phase) {
            var mainErrors = new ErrorsViewModel(_localizationService);

            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = _localizationService.GetLocalizedString("ErrorsWindow.NoFinishing"),
                ErrorElements = [.. _finishingChecker.CheckPhaseContainsFinishing(allFinishing, phase)]
            });
            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = _localizationService.GetLocalizedString("ErrorsWindow.NoBoundaries"),
                ErrorElements = [.. _finishingChecker.CheckFinishingByRoomBounding(allFinishing, phase)]
            });
            string finishingKeyParam = ProjectParamsConfig.Instance.RoomFinishingType.Name;
            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = _localizationService.GetLocalizedString("ErrorsWindow.NoKeyParam"),
                ErrorElements = [.. _finishingChecker.CheckRoomsByKeyParameter(selectedRooms, finishingKeyParam, phase)]
            });

            return mainErrors;
        }

        public ErrorsViewModel CheckFinishingErrors(FinishingCalculator calculator, Phase phase) {
            var finishingErrors = new ErrorsViewModel(_localizationService);

            finishingErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = _localizationService.GetLocalizedString("ErrorsWindow.DifPhases"),
                ErrorElements = [.. _finishingChecker.CheckFinishingByRoom(calculator.FinishingElements, phase)]
            });

            return finishingErrors;
        }

        public WarningsViewModel CheckWarnings(List<Room> selectedRooms,
                                               List<FinishingElement> finishingElements,
                                               Phase phase) {
            var parameterErrors = new WarningsViewModel(_localizationService);

            string numberParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
            parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
                Description = 
                    $"{_localizationService.GetLocalizedString("ErrorsWindow.NoKeyParam")} \"{numberParamName}\"",
                ErrorElements = [.. _finishingChecker.CheckRoomsByParameter(selectedRooms, numberParamName, phase)]
            });
            string nameParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NAME);
            parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
                Description = 
                    $"{_localizationService.GetLocalizedString("ErrorsWindow.NoKeyParam")} \"{nameParamName}\"",
                ErrorElements = [.. _finishingChecker.CheckRoomsByParameter(selectedRooms, nameParamName, phase)]
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
}
