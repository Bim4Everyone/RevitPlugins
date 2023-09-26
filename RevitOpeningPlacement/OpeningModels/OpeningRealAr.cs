using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий чистовое отверстие, идущее на чертежи
    /// </summary>
    internal class OpeningRealAr : ISolidProvider, IEquatable<OpeningRealAr>, IFamilyInstanceProvider {
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
        /// Создает экземпляр класса <see cref="OpeningRealAr"/>
        /// </summary>
        /// <param name="openingReal">Экземпляр семейства чистового отверстия, идущего на чертежи</param>
        public OpeningRealAr(FamilyInstance openingReal) {
            if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
            if(openingReal.Host is null) { throw new ArgumentException($"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент"); }
            _familyInstance = openingReal;

            Id = _familyInstance.Id.IntegerValue;
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningDiameter);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningWidth);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningPlacer.RealOpeningHeight);
            Name = _familyInstance.Name;
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);

            SetTransformedBBoxXYZ();
            SetSolid();
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
        /// </summary>
        public OpeningRealStatus Status { get; set; } = OpeningRealStatus.NotActual;


        public override bool Equals(object obj) {
            return (obj is OpeningRealAr opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return Id;
        }

        public bool Equals(OpeningRealAr other) {
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
        /// Возвращает экземпляр семейства чистового отверстия
        /// </summary>
        /// <returns></returns>
        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }

        /// <summary>
        /// Обновляет свойство <see cref="Status"/>
        /// </summary>
        /// <param name="mepLinkElementsProviders">Коллекция связей с элементами ВИС и заданиями на отверстиями</param>
        public void UpdateStatus(ICollection<IMepLinkElementsProvider> mepLinkElementsProviders) {

            Solid thisOpeningRealSolid = GetSolid();
            Solid thisOpeningRealSolidAfterIntersection = thisOpeningRealSolid;

            foreach(IMepLinkElementsProvider link in mepLinkElementsProviders) {
                ICollection<ElementId> intersectingLinkElements = GetIntersectingLinkElements(link, out Solid thisOpeningRealSolidInLinkCoordinates);
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

        /// <summary>
        /// Возвращает значение параметра, или пустую строку, если параметра у семейства нет. Значения параметров с типом данных "длина" конвертируются в мм и округляются до 1 мм.
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
            if(_familyInstance is null) {
                throw new ArgumentNullException(nameof(_familyInstance));
            }
            string value = string.Empty;
            if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
#if REVIT_2022_OR_GREATER
                if(_familyInstance.GetSharedParam(paramName).Definition.GetDataType() == SpecTypeId.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters)).ToString();
                }
#elif REVIT_2021
                if(_familyInstance.GetSharedParam(paramName).Definition.ParameterType == ParameterType.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), UnitTypeId.Millimeters)).ToString();
                }
#else
                if(_familyInstance.GetSharedParam(paramName).Definition.UnitType == UnitType.UT_Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(GetFamilyInstanceDoubleParamValueOrZero(paramName), DisplayUnitType.DUT_MILLIMETERS)).ToString();
                }
#endif
                object paramValue = _familyInstance.GetParamValue(paramName);
                if(!(paramValue is null)) {
                    if(paramValue is double doubleValue) {
                        value = Math.Round(doubleValue).ToString();
                    } else {
                        value = paramValue.ToString();
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Возвращает значение double параметра экземпляра семейства задания на отверстие в единицах ревита, или 0, если параметр отсутствует
        /// </summary>
        /// <param name="paramName">Название параметра</param>
        /// <returns></returns>
        private double GetFamilyInstanceDoubleParamValueOrZero(string paramName) {
            if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
                return _familyInstance.GetSharedParamValue<double>(paramName);
            } else {
                return 0;
            }
        }
    }
}
