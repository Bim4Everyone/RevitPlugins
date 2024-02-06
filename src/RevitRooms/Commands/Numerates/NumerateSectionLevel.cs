using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Utils.Extensions;

using dosymep.Bim4Everyone.SystemParams;

using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal sealed class NumerateSectionLevel : NumerateCommand {
        private readonly IDictionary<ElementId, int> _ordering;

        private string _levelName;

        public NumerateSectionLevel(RevitRepository revitRepository, IDictionary<ElementId, int> ordering)
            : base(revitRepository) {
            _ordering = ordering;

            RevitParam =
                SystemParamsConfig.Instance.CreateRevitParam(revitRepository.Document, BuiltInParameter.ROOM_NUMBER);
            TransactionName = "Нумерация помещений по секции и этажу";
        }

        protected override SpatialElementViewModel[]
            OrderElements(IEnumerable<SpatialElementViewModel> spatialElements) {
            return spatialElements
                .OrderBy(item => item.RoomSection, _elementComparer)
                .ThenBy(item => item.LevelElevation)
                .ThenBy(item => item.RoomGroup, _elementComparer)
                .ThenBy(item => _ordering.GetValueOrDefault(item.Room.Id, 0))
                .ThenBy(item => GetDistance(item.Element))
                .ToArray();
        }

        protected override NumMode CountFlat(SpatialElementViewModel spatialElement) {
            try {
                // начало нумерации
                // используется стартовое значение
                if(_levelName == null) {
                    return NumMode.NotChange;
                }
                
                // при смене уровня сбрасываем счетчик иначе инкрементируем
                return _levelName?.Equals(GetLevelName(spatialElement)) == true
                ? NumMode.Increment
                : NumMode.Reset;
            } finally {
                _levelName = GetLevelName(spatialElement);
            }
        }

        private string GetLevelName(SpatialElementViewModel spatialElement) {
            return spatialElement.LevelName?.Split('_').FirstOrDefault();
        }
    }
}