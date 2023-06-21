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
        private ElementId _levelId;
        private ElementId _groupId;
        private ElementId _sectionId;
        private string _multiRoom;

        public NumFlatsCommand(RevitRepository revitRepository)
            : base(revitRepository) {
            RevitParam = SharedParamsConfig.Instance.ApartmentNumber;
            TransactionName = "Нумерация групп помещений";
        }

        protected override SpatialElementViewModel[] OrderElements(IEnumerable<SpatialElementViewModel> spatialElements) {
            SpatialElementViewModel[] elements = spatialElements.ToArray();
            return elements
                .OrderBy(item=> item.RoomSection, _elementComparer)
                .ThenBy(item => item, new NumFlatComparer(elements))
                .ThenBy(item=> item.RoomGroup, _elementComparer)
                .ThenBy(item=> GetDistance(item.Element))
                .ToArray();
        }

        protected override bool CountFlat(int currentCount, SpatialElementViewModel spatialElement) {
            // начало нумерации
            // используется стартовое значение
            bool notChangeCount = _levelId == null
                                  && _groupId == null
                                  && _sectionId == null;

            if(string.IsNullOrEmpty(spatialElement.RoomMultilevelGroup)) {
                // если не было изменений счетчик не меняем
                notChangeCount |= _levelId == spatialElement.LevelId
                                  && _groupId == spatialElement.RoomGroup.Id
                                  && _sectionId == spatialElement.RoomSection.Id;

            } else {
                // если не было изменений счетчик не меняем
                notChangeCount |= _groupId == spatialElement.RoomGroup.Id
                                  && _sectionId == spatialElement.RoomSection.Id
                                  && _multiRoom == spatialElement.RoomMultilevelGroup;

            }

            _levelId = spatialElement.LevelId;
            _groupId = spatialElement.RoomGroup.Id;
            _sectionId = spatialElement.RoomSection.Id;
            _multiRoom = spatialElement.RoomMultilevelGroup;

            return !notChangeCount;
        }
    }
}