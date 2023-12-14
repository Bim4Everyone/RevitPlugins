using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие КР, идущее на чертежи. 
    /// Использовать для обертки проемов из активного файла КР
    /// </summary>
    internal class OpeningRealKr : OpeningRealByMep, IEquatable<OpeningRealKr> {
        public OpeningRealKr(FamilyInstance openingReal) : base(openingReal) {
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrDiameter);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrInWallWidth);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningKrPlacer.RealOpeningKrInWallHeight);
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

        /// <summary>
        /// Комментарий
        /// </summary>
        public string Comment { get; } = string.Empty;

        /// <summary>
        /// Статус текущего отверстия относительно полученных заданий
        /// <para>Для обновления использовать метод <see cref="UpdateStatus"/></para>
        /// </summary>
        public OpeningRealStatus Status { get; private set; } = OpeningRealStatus.NotActual;


        public override bool Equals(object obj) {
            return (obj != null) && (obj is OpeningRealKr opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return (int) Id.GetIdValue();
        }

        public bool Equals(OpeningRealKr other) {
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

        /// <summary>
        /// Обновляет свойство <see cref="Status"/>
        /// </summary>
        /// <param name="arLinkElementsProviders">АР связи с входящими заданиями</param>
        public void UpdateStatus(ICollection<IConstructureLinkElementsProvider> arLinkElementsProviders) {
            Solid thisOpeningRealSolid = GetSolid();
            Solid thisOpeningRealSolidAfterIntersection = thisOpeningRealSolid;

            foreach(IConstructureLinkElementsProvider link in arLinkElementsProviders) {
                thisOpeningRealSolidAfterIntersection = SubtractLinkOpenings(
                    link,
                    thisOpeningRealSolidAfterIntersection,
                    out bool openingIsNotActual);
                if(openingIsNotActual) {
                    Status = OpeningRealStatus.NotActual;
                    return;
                }
            }
            var volumeRatio = GetSolidsVolumesRatio(thisOpeningRealSolid, thisOpeningRealSolidAfterIntersection);
            Status = GetStatusByVolumeRatio(volumeRatio);
        }

        /// <summary>
        /// Возвращает коэффициент, показывающий, какую часть исходного солида пересекают элементы из связей
        /// </summary>
        /// <param name="thisOpeningRealSolid">Исходный солид текущего чистового отверстия</param>
        /// <param name="thisOpeningRealSolidAfterIntersection">
        /// Солид текущего чистового отверстия, 
        /// после вычтенных солидов элементов из связей, которые пересекают это отверстие
        /// </param>
        /// <returns>
        /// 0 - элементы из связей на 100% пересекают солид текущего чистового отверстия, 
        /// 1 - солид текущего чистового отверстия не пересекается ни с одним элементом из связи
        /// </returns>
        private double GetSolidsVolumesRatio(Solid thisOpeningRealSolid, Solid thisOpeningRealSolidAfterIntersection) {
            if(thisOpeningRealSolid.Volume == 0) { return 1; }
            return (1 - thisOpeningRealSolidAfterIntersection.Volume / thisOpeningRealSolid.Volume);
        }

        /// <summary>
        /// Возвращает коллекцию всех заданий на отверстия из связи АР, которые пересекаются с текущим отверстием КР
        /// </summary>
        /// <param name="link">Связь АР</param>
        /// <param name="thisOpeningSolidForSubtraction">
        /// Солид текущего отверстия в исходных координатах активного документа, 
        /// из которого вычтены пересекающие его задания на отверстия</param>
        /// <param name="linkOpeningsIntersectConstructions">
        /// Флаг, показывающий, 
        /// полностью ли текущее чистовое отверстие закрывает собой пересекающие его задания на отверстия</param>
        /// <returns></returns>
        private Solid SubtractLinkOpenings(
            IConstructureLinkElementsProvider link,
            Solid thisOpeningSolidForSubtraction,
            out bool linkOpeningsIntersectConstructions) {

            Solid thisOpeningRealSolid = GetSolid();
            Solid thisOpeningSolidInLinkCoordinates = SolidUtils.CreateTransformed(
                thisOpeningRealSolid, link.DocumentTransform.Inverse);
            BoundingBoxXYZ thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ()
                .GetTransformedBoundingBox(link.DocumentTransform.Inverse);

            ICollection<Solid> intersectingTasksSolidsInActiveDocCoordinates = link
                .GetOpeningsReal()
                .Where(openingTask => openingTask.IntersectsSolid(
                    thisOpeningSolidInLinkCoordinates,
                    thisBBoxInLinkCoordinates))
                .Select(task => SolidUtils.CreateTransformed(task.GetSolid(), link.DocumentTransform))
                .ToHashSet();

            Solid solidAfterSubtraction = thisOpeningSolidForSubtraction;
            linkOpeningsIntersectConstructions = false;
            foreach(Solid solid in intersectingTasksSolidsInActiveDocCoordinates) {
                try {
                    solidAfterSubtraction = BooleanOperationsUtils.ExecuteBooleanOperation(
                        solidAfterSubtraction,
                        solid,
                        BooleanOperationsType.Difference);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) { }
                try {
                    linkOpeningsIntersectConstructions =
                        linkOpeningsIntersectConstructions
                        || (BooleanOperationsUtils.ExecuteBooleanOperation(
                            solid,
                            thisOpeningRealSolid,
                            BooleanOperationsType.Difference)?.Volume > 0);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) { }
            }
            return solidAfterSubtraction;
        }

        /// <summary>
        /// Возвращает статус текущего чистового отверстия по коэффициенту пересекаемого объема.
        /// </summary>
        /// <param name="volumeRatio">
        /// Отношение объема солида текущего чистового отверстия, который пересекается с элементами из связей, 
        /// к исходному объему этого солида.
        /// <para>
        /// 0 - элементы из связей на 100% пересекают солид текущего чистового отверстия, 
        /// 1 - солид текущего чистового отверстия не пересекается ни с одним элементом из связи
        /// </para>
        /// </param>
        /// <returns></returns>
        private OpeningRealStatus GetStatusByVolumeRatio(double volumeRatio) {
            if(volumeRatio < 0.01) {
                return OpeningRealStatus.Empty;
            } else if(volumeRatio < 0.5) {
                return OpeningRealStatus.TooBig;
            } else {
                return OpeningRealStatus.Correct;
            }
        }
    }
}
