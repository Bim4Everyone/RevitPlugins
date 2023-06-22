using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone.SystemParams;

using RevitRooms.Comparators;
using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal class NumSectionGroup : NumerateCommand {
        private readonly IDictionary<ElementId, int> _ordering;

        private string _multiRoom;
        private ElementId _levelId;
        private ElementId _groupId;
        private ElementId _sectionId;
        
        public NumSectionGroup(RevitRepository revitRepository, IDictionary<ElementId, int> ordering)
            : base(revitRepository) {
            _ordering = ordering;
            
            RevitParam =
                SystemParamsConfig.Instance.CreateRevitParam(revitRepository.Document, BuiltInParameter.ROOM_NUMBER);
            TransactionName = "Нумерация помещений в пределах каждой секции, этажа и группы";
        }

        protected override SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements) {
            SpatialElementViewModel[] elements = spatialElements.ToArray();
            return elements
                .OrderBy(item => item.RoomSection, _elementComparer)
                .ThenBy(item => item.RoomGroup, _elementComparer)
                .ThenBy(item => item.LevelElevation)
                .ThenBy(item => _ordering.GetValueOrDefault(item.Room.Id, 0))
                .ThenBy(item => GetDistance(item.Element)) // в одной группе может быть несколько гостиных и тп
                .ToArray();
        }

        protected override NumMode CountFlat(SpatialElementViewModel spatialElement) {
            try {
                // начало нумерации
                // используется стартовое значение
                if(_levelId == null
                   && _groupId == null
                   && _sectionId == null) {
                    return NumMode.NotChange;
                }
                
                bool notChangeCount = false;
                if(string.IsNullOrEmpty(spatialElement.RoomMultilevelGroup)) {
                    notChangeCount |= _levelId == spatialElement.LevelId
                                      && _groupId == spatialElement.RoomGroup.Id
                                      && _sectionId == spatialElement.RoomSection.Id;
            
                } else {
                    // у многоуровневых квартир не учитывается уровень
                    notChangeCount |= _groupId == spatialElement.RoomGroup.Id
                                      && _sectionId == spatialElement.RoomSection.Id
                                      && _multiRoom == spatialElement.RoomMultilevelGroup;
            
                }

                // если не было изменений меняем счетчик
                // если были изменения сбрасываем счетчик
                return notChangeCount ? NumMode.Increment : NumMode.Reset;
            } finally {
                _levelId = spatialElement.LevelId;
                _groupId = spatialElement.RoomGroup.Id;
                _sectionId = spatialElement.RoomSection.Id;
                _multiRoom = spatialElement.RoomMultilevelGroup;
            }
        }
    }
}