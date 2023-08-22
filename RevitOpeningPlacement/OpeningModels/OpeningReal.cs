using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие, идущее на чертежи
    /// </summary>
    internal class OpeningReal : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства чистового отверстия
        /// </summary>
        private readonly FamilyInstance _familyInstance;

        /// <summary>
        /// Закэшированный солид
        /// </summary>
        private Solid _solid;

        /// <summary>
        /// Закэшированный BBox
        /// </summary>
        private BoundingBoxXYZ _boundingBox;


        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningReal"/>
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства чистового отверстия, идущего на чертежи</param>
        public OpeningReal(FamilyInstance openingReal) {
            if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
            if(openingReal.Host is null) { throw new ArgumentException($"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент"); }
            _familyInstance = openingReal;
            Id = _familyInstance.Id.IntegerValue;

            SetTransformedBBoxXYZ();
            SetSolid();
        }


        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; private set; }

        /// <summary>
        /// Статус текущего отверстия относительно полученных заданий
        /// </summary>
        public OpeningRealTaskStatus Status { get; set; } = OpeningRealTaskStatus.NotDefined;


        public Solid GetSolid() {
            return _solid;
        }


        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _boundingBox;
        }

        /// <summary>
        /// Возвращает хост экземпляра семейства отверстия
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Element GetHost() {
            var host = _familyInstance.Host;
            if(host is null) {
                throw new ArgumentNullException($"Хост элемента с Id: {_familyInstance.Id} - null");
            }
            return host;
        }


        /// <summary>
        /// Устанавливает значение полю <see cref="_solid"/>
        /// </summary>
        private void SetSolid() {
            XYZ openingLocation = (_familyInstance.Location as LocationPoint).Point;
            var hostElement = GetHost();
            Solid hostSolidCut = hostElement.GetSolid();
            try {
                Solid hostSolidOriginal = (hostElement as HostObject).GetHostElementOriginalSolid();
                var openings = SolidUtils.SplitVolumes(BooleanOperationsUtils.ExecuteBooleanOperation(hostSolidOriginal, hostSolidCut, BooleanOperationsType.Difference));
                var thisOpeningSolid = openings.OrderBy(solidOpening => (solidOpening.ComputeCentroid() - openingLocation).GetLength()).FirstOrDefault();
                if(thisOpeningSolid != null) {
                    _solid = thisOpeningSolid;
                } else {
                    _solid = CreateRawSolid();
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                _solid = CreateRawSolid();
            }
        }

        /// <summary>
        /// Устанавливает значение полю <see cref="_boundingBox"/>
        /// </summary>
        private void SetTransformedBBoxXYZ() {
            _boundingBox = _familyInstance.GetBoundingBox();
        }

        private Solid CreateRawSolid() {
            return GetTransformedBBoxXYZ().CreateSolid();
        }
    }
}
