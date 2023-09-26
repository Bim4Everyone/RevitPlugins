using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Базовый класс полого экземпляра семейства
    /// </summary>
    internal abstract class OpeningRealBase : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства чистового отверстия
        /// </summary>
        private protected readonly FamilyInstance _familyInstance;

        /// <summary>
        /// Закэшированный солид
        /// </summary>
        private protected Solid _solid;

        /// <summary>
        /// Закэшированный BBox
        /// </summary>
        private protected BoundingBoxXYZ _boundingBox;


        /// <summary>
        /// Базовый конструктор, устанавливающий <see cref="_familyInstance"/>, <see cref="_solid"/>, <see cref="_boundingBox"/>
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства проема в стене или перекрытии</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр является null</exception>
        /// <exception cref="ArgumentException">Исключение, если экземпляр семейства не имеет хоста</exception>
        protected OpeningRealBase(FamilyInstance openingReal) {
            if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
            if(openingReal.Host is null) { throw new ArgumentException($"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент"); }
            _familyInstance = openingReal;

            SetTransformedBBoxXYZ();
            SetSolid();
        }


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
        public Element GetHost() {
            return _familyInstance.Host;
        }


        private Solid CreateRawSolid() {
            return GetTransformedBBoxXYZ().CreateSolid();
        }

        /// <summary>
        /// Устанавливает значение полю <see cref="_boundingBox"/>
        /// </summary>
        private void SetTransformedBBoxXYZ() {
            _boundingBox = _familyInstance.GetBoundingBox();
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
    }
}
