using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс для обертки проема из связанного файла АР, подгруженного в активный документ КР
    /// </summary>
    internal class OpeningArTaskIncoming : OpeningRealBase, IEquatable<OpeningArTaskIncoming> {
        /// <summary>
        /// Конструктор класса для обертки проема из связанного файла АР, подгруженного в активный документ КР
        /// </summary>
        public OpeningArTaskIncoming(FamilyInstance openingReal, Transform transform) : base(openingReal) {
            FileName = _familyInstance.Document.PathName;
            Id = _familyInstance.Id.IntegerValue;

            Transform = transform;
            Location = Transform.OfPoint((_familyInstance.Location as LocationPoint).Point);
            OpeningType = RevitRepository.GetOpeningType(_familyInstance.Symbol.FamilyName);

            DisplayDiameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArDiameter);
            DisplayWidth = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArWidth);
            DisplayHeight = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArHeight);
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);

            Diameter = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningPlacer.RealOpeningArDiameter);
            Height = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningPlacer.RealOpeningArHeight);
            Width = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningPlacer.RealOpeningArWidth);
        }


        public string FileName { get; }

        public int Id { get; }

        /// <summary>
        /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
        /// </summary>
        public Transform Transform { get; } = Transform.Identity;

        /// <summary>
        /// Точка расположения экземпляра семейства входящего задания на отверстие в координатах активного документа - получателя заданий
        /// </summary>
        public XYZ Location { get; }

        public string Comment { get; } = string.Empty;

        public OpeningType OpeningType { get; } = OpeningType.WallRectangle;

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
            return Id + FileName.GetHashCode();
        }

        public bool Equals(OpeningArTaskIncoming other) {
            return (other != null) && (Id == other.Id) && FileName.Equals(other.FileName, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Возвращает солид архитектурного проема в координатах активного файла (КР) - получателя заданий на отверстия
        /// </summary>
        /// <returns></returns>
        public override Solid GetSolid() {
            return SolidUtils.CreateTransformed(_solid, Transform);
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
        /// <param name="realOpenings">Коллекция чистовых отверстий КР, размещенных в активном КР документе-получателе заданий на отверстия</param>
        /// <param name="constructureElementsIds">Коллекция элементов конструкций из активного документа-получателя заданий</param>
        public void UpdateStatus(ICollection<OpeningRealKr> realOpenings, ICollection<ElementId> constructureElementsIds) {

        }
    }
}
