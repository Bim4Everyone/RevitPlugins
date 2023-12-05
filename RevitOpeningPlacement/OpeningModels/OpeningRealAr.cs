using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие АР, идущее на чертежи. 
    /// Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
    /// </summary>
    internal class OpeningRealAr : OpeningRealByMep, IEquatable<OpeningRealAr> {
        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningRealAr"/>. 
        /// Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства чистового отверстия АР, идущего на чертежи</param>
        public OpeningRealAr(FamilyInstance openingReal) : base(openingReal) {
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArDiameter);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArWidth);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArHeight);
            Name = _familyInstance.Name;
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);
        }


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

        public string Name { get; } = string.Empty;

        public string Comment { get; } = string.Empty;

        /// <summary>
        /// Статус текущего отверстия относительно полученных заданий
        /// <para>Для обновления использовать метод <see cref="UpdateStatus"/></para>
        /// </summary>
        public OpeningRealStatus Status { get; private set; } = OpeningRealStatus.NotActual;


        public override bool Equals(object obj) {
            return (obj is OpeningRealAr opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return (int) Id.GetIdValue();
        }

        public bool Equals(OpeningRealAr other) {
            return (other != null) && (Id == other.Id);
        }

        public override Solid GetSolid() {
            return GetOpeningSolid();
        }

        public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _boundingBox;
        }

        /// <summary>
        /// Обновляет свойство <see cref="Status"/>
        /// </summary>
        /// <param name="mepLinkElementsProviders">Коллекция связей с элементами ВИС и заданиями на отверстиями</param>
        public void UpdateStatus(ICollection<IMepLinkElementsProvider> mepLinkElementsProviders) {
            Status = DetermineStatus(mepLinkElementsProviders);
        }
    }
}
