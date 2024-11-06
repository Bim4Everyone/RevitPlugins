using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class DeclarationProject {
        private protected readonly RevitDocumentViewModel _document;
        private protected readonly DeclarationSettings _settings;
        private protected readonly RevitRepository _revitRepository;

        private protected readonly Phase _phase;

        private protected readonly IEnumerable<RoomElement> _rooms;
        private protected IReadOnlyCollection<RoomGroup> _roomGroups;

        public DeclarationProject(RevitDocumentViewModel document,
                                  RevitRepository revitRepository,
                                  DeclarationSettings settings) {
            _document = document;
            _settings = settings;
            _revitRepository = revitRepository;

            _phase = revitRepository.GetPhaseByName(document.Document, _settings.SelectedPhase.Name);

            _rooms = revitRepository.GetRoomsOnPhase(document.Document, _phase, settings);
            _rooms = FilterDeclarationRooms(_rooms);
        }

        public RevitDocumentViewModel Document => _document;
        public Phase Phase => _phase;

        /// <summary>Отфильтрованные помещения для выгрузки</summary>
        public IEnumerable<RoomElement> Rooms => _rooms;
        public IReadOnlyCollection<RoomGroup> RoomGroups => _roomGroups;

        private IReadOnlyCollection<RoomElement> FilterDeclarationRooms(IEnumerable<RoomElement> rooms) {
            Parameter filterParam = _settings.FilterRoomsParam;
            string[] filterValues = _settings.FilterRoomsValues;
            StringComparer strComparer = StringComparer.OrdinalIgnoreCase;

            return rooms
                .Where(x => filterValues.Contains(x.GetTextParamValue(filterParam), strComparer))
                .ToList();
        }

        public ErrorsListViewModel CheckRoomGroupsInRpoject() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "В проекте отсутствуют необходимые группы помещений на выбранной стадии",
                DocumentName = _document.Name
            };

            if(_roomGroups.Count == 0) {
                errorListVM.Errors = new List<ErrorElement>() {
                    new ErrorElement(_settings.SelectedPhase.Name, "Отсутствуют группы помещений")
                };
            }

            return errorListVM;
        }

        public ErrorsListViewModel CheckActualRoomAreas() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "Не актуальные площади помещений, рассчитанные квартирографией",
                DocumentName = _document.Name
            };

            foreach(RoomGroup roomGroup in _roomGroups) {
                if(!roomGroup.CheckActualRoomAreas()) {
                    string groupInfo = $"Квартира № {roomGroup.Number} на этаже {roomGroup.Level}";
                    string groupAreas = "Площади помещений, рассчитанные квартирографией " +
                        "отличаются от актуальной системной площадей помещения.";
                    errorListVM.Errors.Add(new ErrorElement(groupInfo, groupAreas));
                }
            }

            return errorListVM;
        }
    }
}
