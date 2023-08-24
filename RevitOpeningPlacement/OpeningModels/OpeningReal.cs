using System;
using System.Collections.Generic;
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
    internal class OpeningReal : ISolidProvider, IEquatable<OpeningReal> {
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
        public OpeningRealTaskStatus Status { get; set; } = OpeningRealTaskStatus.NotActual;


        public override bool Equals(object obj) {
            return (obj is OpeningReal opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return Id;
        }

        public bool Equals(OpeningReal other) {
            return (other != null) && (Id == other.Id);
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

        /// <summary>
        /// Обновляет свойство <see cref="Status"/>
        /// </summary>
        /// <param name="openingsRealInActiveDoc">Коллекция чистовых отверстий их активного документа</param>
        /// <param name="mepLinkElementsProviders">Коллекция связей с элементами ВИС и заданиями на отверстиями</param>
        public void UpdateStatus(
            ref ICollection<OpeningReal> openingsRealInActiveDoc,
            ICollection<IMepLinkElementsProvider> mepLinkElementsProviders) {

            Solid thisOpeningRealSolid = GetSolid();

            foreach(var link in mepLinkElementsProviders) {
                ICollection<ElementId> intersectingLinkElements = GetIntersectingLinkElements(link, out Solid thisOpeningRealSolidInLinkCoordinates);
                if(intersectingLinkElements.Count > 0) {
                    if(LinkElementsIntersectHost(link, intersectingLinkElements)) {
                        Status = OpeningRealTaskStatus.NotActual;
                        return;
                    }


                }
            }
        }

        /// <summary>
        /// Возвращает коллекцию солидов заданных элементов из связанного файла в координатах файла c текущим чистовым отверстием
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
        /// <param name="thisOpeningRealSolidInLinkCoordinates">Солид текущего чистового отверстия в координатах связанного файла</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingLinkElements(IMepLinkElementsProvider mepLink, out Solid thisOpeningRealSolidInLinkCoordinates) {
            thisOpeningRealSolidInLinkCoordinates = SolidUtils.CreateTransformed(GetSolid(), mepLink.DocumentTransform.Inverse);

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
            return new FilteredElementCollector(mepLink.Document, mepLink.GetMepElementIds())
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
            return new FilteredElementCollector(mepLink.Document, mepLink.GetOpeningsTaskIds())
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningRealSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningRealSolidInLinkCoordinates))
                .ToElementIds();
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
