using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие КР, идущее на чертежи. Использовать для обертки проемов из активного файла КР
    /// </summary>
    internal class OpeningRealKr : OpeningRealBase, IEquatable<OpeningRealKr> {
        public OpeningRealKr(FamilyInstance openingReal) : base(openingReal) {
            Id = _familyInstance.Id.IntegerValue;
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningKrDiameter);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningKrInWallWidth);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningKrInWallHeight);
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);
        }


        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Диаметр в мм, если есть
        /// </summary>
        public string Diameter { get; } = string.Empty;

        /// <summary>
        /// Ширина в мм, если есть
        /// </summary>
        public string Width { get; } = string.Empty;

        /// <summary>
        /// Высота в мм, если есть
        /// </summary>
        public string Height { get; } = string.Empty;

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment { get; } = string.Empty;

        /// <summary>
        /// Статус текущего отверстия относительно полученных заданий
        /// </summary>
        public OpeningRealStatus Status { get; set; } = OpeningRealStatus.NotActual;


        public override bool Equals(object obj) {
            return (obj != null) && (obj is OpeningRealKr opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return Id;
        }

        public bool Equals(OpeningRealKr other) {
            return (other != null) && (Id == other.Id);
        }

        public override Solid GetSolid() {
            return _solid;
        }

        public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _boundingBox;
        }

        /// <summary>
        /// Обновляет свойство <see cref="Status"/>
        /// </summary>
        /// <param name="arLinkElementsProviders">АР связи с входящими заданиями</param>
        public void UpdateStatus(ICollection<IConstructureLinkElementsProvider> arLinkElementsProviders) {

        }
    }
}
