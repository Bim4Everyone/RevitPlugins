using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие АР, идущее на чертежи. Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
    /// </summary>
    internal class OpeningRealAr : OpeningRealBase, IEquatable<OpeningRealAr> {
        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningRealAr"/>. Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства чистового отверстия АР, идущего на чертежи</param>
        public OpeningRealAr(FamilyInstance openingReal) : base(openingReal) {
            Id = _familyInstance.Id.IntegerValue;
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArDiameter);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArWidth);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningArHeight);
            Name = _familyInstance.Name;
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
            return Id;
        }

        public bool Equals(OpeningRealAr other) {
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
        /// <param name="mepLinkElementsProviders">Коллекция связей с элементами ВИС и заданиями на отверстиями</param>
        public void UpdateStatus(ICollection<IMepLinkElementsProvider> mepLinkElementsProviders) {

            Solid thisOpeningRealSolid = GetSolid();
            Solid thisOpeningRealSolidAfterIntersection = thisOpeningRealSolid;

            foreach(IMepLinkElementsProvider link in mepLinkElementsProviders) {
                ICollection<ElementId> intersectingLinkElements = GetIntersectingLinkElements(link);
                if(intersectingLinkElements.Count > 0) {
                    if(LinkElementsIntersectHost(link, intersectingLinkElements)) {
                        Status = OpeningRealStatus.NotActual;
                        return;
                    }

                    ICollection<Solid> linkElementsSolids = GetLinkElementsSolids(link, intersectingLinkElements);
                    thisOpeningRealSolidAfterIntersection = SubtractLinkSolids(thisOpeningRealSolidAfterIntersection, linkElementsSolids);
                }
            }
            double volumeRatio = GetSolidsVolumesRatio(thisOpeningRealSolid, thisOpeningRealSolidAfterIntersection);
            OpeningRealStatus status = GetStatusByVolumeRatio(volumeRatio);
            Status = status;
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
            } else if(volumeRatio < 0.2) {
                return OpeningRealStatus.TooBig;
            } else {
                return OpeningRealStatus.Correct;
            }
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
        /// Вычитает солиды связанных элементов из солида текущего отверстия и возвращает полученный новый солид
        /// </summary>
        /// <param name="thisOpeningRealSolid">Солид текущего чистового отверстия в координатах своего файла</param>
        /// <param name="linkSolidsInThisCoordinates">Коллекция солидов элементов из связи в координатах файла с чистовыми отверстиями</param>
        /// <returns></returns>
        private Solid SubtractLinkSolids(Solid thisOpeningRealSolid, ICollection<Solid> linkSolidsInThisCoordinates) {
            var thisOpeningRealSolidAfterIntersection = thisOpeningRealSolid;
            foreach(Solid linkSolid in linkSolidsInThisCoordinates) {
                try {
                    thisOpeningRealSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                        thisOpeningRealSolidAfterIntersection,
                        linkSolid,
                        BooleanOperationsType.Difference);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    continue;
                }
            }
            return thisOpeningRealSolidAfterIntersection;
        }

        /// <summary>
        /// Возвращает коллекцию солидов заданных элементов из связанного файла в координатах активного файла c текущим чистовым отверстием
        /// </summary>
        /// <param name="mepLink">Связанный файл с элементами ВИС и заданиями на отверстия</param>
        /// <param name="linkElementsIds">Id элементов из связанного файла, из которых надо получить солиды</param>
        /// <returns></returns>
        private ICollection<Solid> GetLinkElementsSolids(IMepLinkElementsProvider mepLink, ICollection<ElementId> linkElementsIds) {
            var doc = mepLink.Document;
            return linkElementsIds.Select(id => SolidUtils.CreateTransformed(doc.GetElement(id).GetSolid(), mepLink.DocumentTransform)).ToHashSet();
        }

        /// <summary>
        /// Проверяет, пересекаются ли заданные элементы из связи с хостом текущего чистового отверстия
        /// </summary>
        /// <param name="mepLink">Связанный файл с элементами ВИС и заданиями на отверстия</param>
        /// <param name="linkElementsForChecking">Элементы из связи для проверки на пересечение</param>
        /// <returns>True, если хотя бы 1 элемент пересекается с хостом текущего чистового отверстия, иначе False</returns>
        private bool LinkElementsIntersectHost(IMepLinkElementsProvider mepLink, ICollection<ElementId> linkElementsForChecking) {
            if(!linkElementsForChecking.Any()) {
                return false;
            }
            var hostSolidInLinkCoordinates = SolidUtils.CreateTransformed(GetHost().GetSolid(), mepLink.DocumentTransform.Inverse);
            return new FilteredElementCollector(mepLink.Document, linkElementsForChecking)
                .WherePasses(new BoundingBoxIntersectsFilter(hostSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(hostSolidInLinkCoordinates))
                .Any();
        }

        /// <summary>
        /// Возвращает коллекцию элементов ВИС и элементов заданий на отверстия из связанного файла
        /// </summary>
        /// <param name="mepLink">Связанный файл с элементами ВИС и заданиями на отверстия</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingLinkElements(IMepLinkElementsProvider mepLink) {
            Solid thisOpeningRealSolidInLinkCoordinates = SolidUtils.CreateTransformed(GetSolid(), mepLink.DocumentTransform.Inverse);

            var mepElements = GetIntersectingLinkMepElements(mepLink, thisOpeningRealSolidInLinkCoordinates).ToHashSet();
            var openingTasks = GetIntersectingLinkOpeningTasks(mepLink, thisOpeningRealSolidInLinkCoordinates);
            mepElements.UnionWith(openingTasks);

            return mepElements;
        }

        /// <summary>
        /// Возвращает коллекцию элементов ВИС из связи, которые пересекаются с солидом текущего чистового отверстия
        /// </summary>
        /// <param name="mepLink">Связанный файл с элементами ВИС и заданиями на отверстия</param>
        /// <param name="thisOpeningRealSolidInLinkCoordinates">Солид текущего чистового отверстия в координатах связанного файла</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingLinkMepElements(IMepLinkElementsProvider mepLink, Solid thisOpeningRealSolidInLinkCoordinates) {
            var ids = mepLink.GetMepElementIds();
            if(!ids.Any()) {
                return Array.Empty<ElementId>();
            }
            return new FilteredElementCollector(mepLink.Document, ids)
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningRealSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningRealSolidInLinkCoordinates))
                .ToElementIds();
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров семейств заданий на отверстия из связи, которые пересекаются с солидом текущего чистового отверстия
        /// </summary>
        /// <param name="mepLink">Связанный файл с элементами ВИС и заданиями на отверстия</param>
        /// <param name="thisOpeningRealSolidInLinkCoordinates">Солид текущего чистового отверстия в координатах связанного файла</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingLinkOpeningTasks(IMepLinkElementsProvider mepLink, Solid thisOpeningRealSolidInLinkCoordinates) {
            ICollection<ElementId> ids = mepLink.GetOpeningsTaskIds();
            if(!ids.Any()) {
                return Array.Empty<ElementId>();
            }
            return new FilteredElementCollector(mepLink.Document, ids)
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningRealSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningRealSolidInLinkCoordinates))
                .ToElementIds();
        }
    }
}
