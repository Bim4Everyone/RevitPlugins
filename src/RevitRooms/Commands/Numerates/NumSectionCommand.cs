using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone.SystemParams;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal class NumSectionCommand : NumerateCommand {
        private readonly IDictionary<ElementId, int> _ordering;

        private object _first;
        
        public NumSectionCommand(RevitRepository revitRepository, IDictionary<ElementId, int> ordering)
            : base(revitRepository) {
            _ordering = ordering;
            
            RevitParam =
                SystemParamsConfig.Instance.CreateRevitParam(revitRepository.Document, BuiltInParameter.ROOM_NUMBER);
            TransactionName = "Нумерация помещений по секции";
        }

        protected override SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements) {
            return spatialElements
                .OrderBy(item => item.RoomSection, _elementComparer)
                .ThenBy(item => item.LevelElevation)
                .ThenBy(item => item.RoomGroup, _elementComparer)
                .ThenBy(item => _ordering.GetValueOrDefault(item.Room.Id, 0))
                .ThenBy(item => GetDistance(item.Element))
                .ToArray();
        }
        
        protected override NumMode CountFlat(SpatialElementViewModel spatialElement) {
            // начало нумерации
            // используется стартовое значение
            if(_first == null) {
                _first = new object();
                return NumMode.NotChange;
            }
                
            // инкрементируем счетчик
            return NumMode.Increment;
        }
    }
}