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

namespace RevitFinishing.Models
{
    internal class ProjectChecker {
        private readonly FinishingChecker _finishingChecker;
        private readonly Phase _phase;
        private readonly ILocalizationService _localizationService;

        public ProjectChecker(Phase phase, ILocalizationService localizationService) {
            _phase = phase;
            _finishingChecker = new FinishingChecker(phase);
            _localizationService = localizationService;
        }

        public ErrorsViewModel CheckMainErrors(FinishingInProject allFinishing, 
                                               List<Room> selectedRooms) {
            ErrorsViewModel mainErrors = new ErrorsViewModel(_localizationService);

            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = "На выбранной стадии не найдены экземпляры отделки",
                ErrorElements = [.. _finishingChecker.CheckPhaseContainsFinishing(allFinishing)]
            });
            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = "Экземпляры отделки являются границами помещений",
                ErrorElements = [.. _finishingChecker.CheckFinishingByRoomBounding(allFinishing)]
            });
            string finishingKeyParam = ProjectParamsConfig.Instance.RoomFinishingType.Name;
            mainErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = "У помещений не заполнен ключевой параметр отделки",
                ErrorElements = [.. _finishingChecker.CheckRoomsByKeyParameter(selectedRooms, finishingKeyParam)]
            });

            return mainErrors;
        }

        public ErrorsViewModel CheckFinishingErrors(FinishingCalculator calculator) {
            ErrorsViewModel finishingErrors = new ErrorsViewModel(_localizationService);

            finishingErrors.AddElements(new ErrorsListViewModel(_localizationService) {
                Description = "Элементы отделки относятся к помещениям с разными типами отделки",
                ErrorElements = [.. _finishingChecker.CheckFinishingByRoom(calculator.FinishingElements)]
            });

            return finishingErrors;
        }

        public WarningsViewModel CheckWarnings(List<Room> selectedRooms,
                                               List<FinishingElement> finishingElements) {
            WarningsViewModel parameterErrors = new WarningsViewModel(_localizationService);

            string numberParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER);
            parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
                Description = $"У помещений не заполнен параметр \"{numberParamName}\"",
                ErrorElements = [.. _finishingChecker.CheckRoomsByParameter(selectedRooms, numberParamName)]
            });
            string nameParamName = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NAME);
            parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
                Description = $"У помещений не заполнен параметр \"{nameParamName}\"",
                ErrorElements = [.. _finishingChecker.CheckRoomsByParameter(selectedRooms, nameParamName)]
            });
            parameterErrors.AddElements(new WarningsListViewModel(_localizationService) {
                Description = $"Пользовательские семейства",
                ErrorElements = [.. finishingElements
                    .Where(x => x.IsCustomFamily)
                    .Select(x => new NoticeElementViewModel(x.RevitElement, _phase.Name))]
            });

            return parameterErrors;
        }
    }
}
