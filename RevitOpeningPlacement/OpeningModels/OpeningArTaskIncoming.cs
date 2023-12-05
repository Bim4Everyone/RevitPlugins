using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс для обертки проема из связанного файла АР, подгруженного в активный документ КР
    /// </summary>
    internal class OpeningArTaskIncoming : OpeningRealBase, IEquatable<OpeningArTaskIncoming>, IOpeningTaskIncoming {
        private readonly RevitRepository _revitRepository;

        /// <summary>
        /// Конструктор класса для обертки проема из связанного файла АР, подгруженного в активный документ КР
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа КР</param>
        /// <param name="openingTask">Задание на отверстие от АР</param>
        /// <param name="transform">
        /// Трансформация файла задания на отверстие от АР относительно активного документа КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public OpeningArTaskIncoming(RevitRepository revitRepository, FamilyInstance openingTask, Transform transform)
            : base(openingTask) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            FileName = _familyInstance.Document.PathName;
            Transform = transform;
            Location = Transform.OfPoint((_familyInstance.Location as LocationPoint).Point);
            // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
            Rotation = (_familyInstance.Location as LocationPoint).Rotation
                + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);
            OpeningType = RevitRepository.GetOpeningType(_familyInstance.Symbol.FamilyName);

            DisplayDiameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArDiameter);
            DisplayWidth = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArWidth);
            DisplayHeight = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArHeight);
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);

            Diameter = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArDiameter);
            Height = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArHeight);
            Width = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArWidth);
        }


        public string FileName { get; }


        /// <summary>
        /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
        /// </summary>
        public Transform Transform { get; } = Transform.Identity;

        /// <summary>
        /// Точка расположения экземпляра семейства входящего задания на отверстие 
        /// в координатах активного документа - получателя заданий
        /// </summary>
        public XYZ Location { get; }

        /// <summary>
        /// Угол поворота задания на отверстие в радианах в координатах активного файла, 
        /// в который подгружена связь с заданием на отверстие от АР
        /// </summary>
        public double Rotation { get; }

        public string Comment { get; } = string.Empty;

        public OpeningType OpeningType { get; } = OpeningType.WallRectangle;

        /// <summary>
        /// Статус входящего задания на отверстие от АР
        /// <para>Для обновления использовать метод <see cref="UpdateStatus"/></para>
        /// </summary>
        public OpeningTaskIncomingStatus Status { get; private set; } = OpeningTaskIncomingStatus.New;

        /// <summary>
        /// Значение диаметра в мм. Если диаметра у отверстия нет, будет пустая строка.
        /// </summary>
        public string DisplayDiameter { get; } = string.Empty;

        /// <summary>
        /// Значение ширины в мм. Если ширины у отверстия нет, будет пустая строка.
        /// </summary>
        public string DisplayWidth { get; } = string.Empty;

        /// <summary>
        /// Значение высоты в мм. Если высоты у отверстия нет, будет пустая строка.
        /// </summary>
        public string DisplayHeight { get; } = string.Empty;

        /// <summary>
        /// Диаметр в единицах ревита или 0, если диаметра нет
        /// </summary>
        public double Diameter { get; } = 0;

        /// <summary>
        /// Ширина в единицах ревита или 0, если ширины нет
        /// </summary>
        public double Width { get; } = 0;

        /// <summary>
        /// Высота в единицах ревита или 0, если высоты нет
        /// </summary>
        public double Height { get; } = 0;


        public override bool Equals(object obj) {
            return (obj is OpeningArTaskIncoming opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return (int) Id.GetIdValue() + FileName.GetHashCode();
        }

        public bool Equals(OpeningArTaskIncoming other) {
            return (other != null)
                && (Id == other.Id)
                && FileName.Equals(other.FileName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Возвращает солид архитектурного проема в координатах активного файла (КР) - получателя заданий на отверстия
        /// </summary>
        /// <returns></returns>
        public override Solid GetSolid() {
            return SolidUtils.CreateTransformed(GetOpeningSolid(), Transform);
        }

        /// <summary>
        /// Возвращает BBox в координатах активного документа-получателя (КР) заданий на отверстия
        /// </summary>
        /// <returns></returns>
        public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return GetSolid().GetTransformedBoundingBox();
        }

        /// <summary>
        /// Обновляет <see cref="Status"/> входящего задания на отверстие
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий КР, 
        /// размещенных в активном КР документе-получателе заданий на отверстия</param>
        /// <param name="constructureElementsIds">
        /// Коллекция элементов конструкций из активного документа-получателя заданий</param>
        public void UpdateStatus(
            ICollection<OpeningRealKr> realOpenings,
            ICollection<ElementId> constructureElementsIds) {
            var thisOpeningSolid = GetSolid();
            var thisOpeningBBox = GetTransformedBBoxXYZ();

            var intersectingStructureElements = GetIntersectingStructureElementsIds(
                thisOpeningSolid,
                constructureElementsIds);
            var intersectingOpenings = GetIntersectingOpeningsIds(realOpenings, thisOpeningSolid, thisOpeningBBox);

            if((intersectingStructureElements.Count == 0) && (intersectingOpenings.Count == 0)) {
                Status = OpeningTaskIncomingStatus.NoIntersection;
            } else if((intersectingStructureElements.Count > 0) && (intersectingOpenings.Count == 0)) {
                Status = OpeningTaskIncomingStatus.New;
            } else if((intersectingStructureElements.Count > 0) && (intersectingOpenings.Count > 0)) {
                Status = OpeningTaskIncomingStatus.NotMatch;
            } else if((intersectingStructureElements.Count == 0) && (intersectingOpenings.Count > 0)) {
                Status = OpeningTaskIncomingStatus.Completed;
            }
        }


        /// <summary>
        /// Возвращает коллекцию элементов конструкций из активного документа, 
        /// с которыми пересекается текущее задание на отверстие из связи
        /// </summary>
        /// <param name="thisOpeningSolid">
        /// Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="constructureElementsIds">
        /// Коллекция id элементов конструкций из активного документа ревита</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingStructureElementsIds(
            Solid thisOpeningSolid,
            ICollection<ElementId> constructureElementsIds) {
            if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0) || (!constructureElementsIds.Any())) {
                return Array.Empty<ElementId>();
            } else {
                return new FilteredElementCollector(_revitRepository.Doc, constructureElementsIds)
                    .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                    .ToElementIds();
            }
        }

        /// <summary>
        /// Возвращает коллекцию Id проемов из активного документа, 
        /// которые пересекаются с текущим заданием на отверстие из связи
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий из активного документа ревита</param>
        /// <param name="thisOpeningSolid">
        /// Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="thisOpeningBBox">
        /// Бокс текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingOpeningsIds(
            ICollection<OpeningRealKr> realOpenings,
            Solid thisOpeningSolid,
            BoundingBoxXYZ thisOpeningBBox) {
            if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0)) {
                return Array.Empty<ElementId>();
            } else {
                var opening = realOpenings.FirstOrDefault(
                    realOpening => realOpening.IntersectsSolid(thisOpeningSolid, thisOpeningBBox));
                if(opening != null) {
                    return new ElementId[] { opening.Id };
                } else {
                    return Array.Empty<ElementId>();
                }
            }
        }
    }
}
