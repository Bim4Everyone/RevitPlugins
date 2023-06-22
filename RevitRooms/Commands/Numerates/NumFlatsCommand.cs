using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.Native;
using DevExpress.Utils.Extensions;
using DevExpress.XtraSpellChecker;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitRooms.Comparators;
using RevitRooms.Models;
using RevitRooms.ViewModels;

namespace RevitRooms.Commands.Numerates {
    internal class NumFlatsCommand : NumerateCommand {
        private string _multiRoom;
        private ElementId _levelId;
        private ElementId _groupId;
        private ElementId _sectionId;

        public NumFlatsCommand(RevitRepository revitRepository)
            : base(revitRepository) {
            RevitParam = SharedParamsConfig.Instance.ApartmentNumber;
            TransactionName = "Нумерация групп помещений";
        }

        protected override SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements) {
            SpatialElementViewModel[] elements = spatialElements.ToArray();
            return elements
                .OrderBy(item => item.RoomSection, _elementComparer)
                .ThenBy(item => item, new NumFlatComparer(elements))
                .ThenBy(item => item.RoomGroup, _elementComparer)
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

                // если не было изменений не меняем счетчик
                // если были изменения инкрементируем счетчик               
                return notChangeCount ? NumMode.NotChange : NumMode.Increment;
            } finally {
                _levelId = spatialElement.LevelId;
                _groupId = spatialElement.RoomGroup.Id;
                _sectionId = spatialElement.RoomSection.Id;
                _multiRoom = spatialElement.RoomMultilevelGroup;
            }
        }
    }
}