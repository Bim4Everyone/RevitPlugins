using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия из связанного файла-задания на отверстия,
    /// подгруженного в текущий документ получателя
    /// </summary>
    internal class OpeningMepTaskIncoming : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие из связанного файла
        /// </summary>
        private readonly FamilyInstance _familyInstance;

        /// <summary>
        /// Репозиторий текущего документа, в который подгружен связанный документ с заданиями на отверстия
        /// </summary>
        private readonly RevitRepository _revitRepository;


        /// <summary>
        /// Экземпляр семейства задания на отверстие, расположенного в связанном файле задания на отверстия
        /// 
        /// <para>Примечание: конструктор не обновляет свойство <see cref="Status"/>. Для обновления этого свойства надо вызвать <see cref="UpdateStatusAndHostName"/></para>
        /// </summary>
        /// <param name="openingTaskIncoming">Экземпляр семейства задания на отверстие из связанного файла</param>
        /// <param name="revitRepository">Репозиторий текущего документа, в который подгружен документ с заданиями на отверстия</param>
        /// <param name="transform">Трансформация связанного файла, в котором создано задание на отверстие</param>
        public OpeningMepTaskIncoming(FamilyInstance openingTaskIncoming, RevitRepository revitRepository, Transform transform) {
            if(openingTaskIncoming is null) {
                throw new ArgumentNullException(nameof(openingTaskIncoming));
            }
            _familyInstance = openingTaskIncoming;
            _revitRepository = revitRepository;

            Id = _familyInstance.Id.IntegerValue;
            Location = (_familyInstance.Location as LocationPoint).Point;
            FileName = _familyInstance.Document.PathName;

            Transform = transform;

            Date = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDate);
            MepSystem = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningMepSystem);
            Description = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDescription);
            CenterOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetCenter);
            BottomOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetBottom);
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDiameter);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningHeight);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningWidth);
            Thickness = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningThickness);

            string[] famNameParts = _familyInstance.Symbol.FamilyName.Split('_');
            if(famNameParts.Length > 0) {
                FamilyShortName = famNameParts.Last();
            }
        }


        public string FileName { get; }

        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; }

        /// <summary>
        /// Комментарий к входящему заданию на отверстие
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, обозначающий, принято ли задание получателем, или нет
        /// </summary>
        public bool IsAccepted { get; set; } = true;

        public string Date { get; } = string.Empty;

        public string MepSystem { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public string CenterOffset { get; } = string.Empty;

        public string BottomOffset { get; } = string.Empty;

        public string Diameter { get; } = string.Empty;

        public string Width { get; } = string.Empty;

        public string Height { get; } = string.Empty;

        public string Thickness { get; } = string.Empty;

        /// <summary>
        /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
        /// </summary>
        public Transform Transform { get; } = Transform.Identity;

        /// <summary>
        /// Короткое обозначение семейства задания на отверстие
        /// </summary>
        public string FamilyShortName { get; } = string.Empty;

        /// <summary>
        /// Название элемента, в котором расположено задание на отверстие. Предназначено для дальнейшей сортировки входящих заданий в навигаторе по типам стен: штукатурка/монолит/кладка и т.п.
        /// </summary>
        public string HostName { get; private set; } = string.Empty;

        /// <summary>
        /// Статус отработки задания на отверстие
        /// </summary>
        public OpeningTaskIncomingStatus Status { get; set; } = OpeningTaskIncomingStatus.New;


        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }

        /// <summary>
        /// Возвращает солид входящего задания на отверстие в координатах активного файла - получателя заданий на отверстия
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Solid GetSolid() {
            Solid famInstSolid = _familyInstance.GetSolid();
            if(famInstSolid?.Volume > 0) {
                return SolidUtils.CreateTransformed(_familyInstance.GetSolid(), Transform);
            } else {
                throw new ArgumentException($"Не удалось обработать задание на отверстие с ID {Id} из файла \'{FileName}\'");
            }
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _familyInstance.GetBoundingBox().TransformBoundingBox(Transform);
        }

        private string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
            if(_familyInstance is null) {
                throw new ArgumentNullException(nameof(_familyInstance));
            }
            string value = string.Empty;
            //if(_familyInstance.IsExistsSharedParam(paramName)) {
            if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
#if REVIT_2022_OR_GREATER
                if(_familyInstance.GetSharedParam(paramName).Definition.GetDataType() == SpecTypeId.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), UnitTypeId.Millimeters)).ToString();
                }
#elif REVIT_2021
                if(_familyInstance.GetSharedParam(paramName).Definition.ParameterType == ParameterType.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), UnitTypeId.Millimeters)).ToString();
                }
#else
                if(_familyInstance.GetSharedParam(paramName).Definition.UnitType == UnitType.UT_Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), DisplayUnitType.DUT_MILLIMETERS)).ToString();
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
        /// Обновляет <see cref="Status"/> и <see cref="HostName"/> входящего задания на отверстие
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий, размещенных в активном документе-получателе заданий на отверстия</param>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateStatusAndHostName(ICollection<OpeningReal> realOpenings, ICollection<ElementId> constructureElementsIds) {
            var thisOpeningSolid = GetSolid();
            var thisOpeningBBox = GetTransformedBBoxXYZ();

            var intersectingStructureElements = GetIntersectingStructureElementsIds(thisOpeningSolid, constructureElementsIds);
            var intersectingOpenings = GetIntersectingOpeningsIds(realOpenings, thisOpeningSolid, thisOpeningBBox);
            var hostId = GetOpeningTaskHostId(intersectingStructureElements, intersectingOpenings);
            SetOpeningTaskHostName(hostId);

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
        /// Возвращает коллекцию элементов конструкций, с которыми пересекается текущее задание на отверстие
        /// </summary>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="constructureElementsIds">Коллекция id элементов конструкций из активного документа ревита, для которых были сделаны задания на отверстия</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingStructureElementsIds(Solid thisOpeningSolid, ICollection<ElementId> constructureElementsIds) {
            if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0)) {
                return Array.Empty<ElementId>();
            } else {
                return new FilteredElementCollector(_revitRepository.Doc, constructureElementsIds)
                    .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                    .ToElementIds();
            }
        }

        /// <summary>
        /// Возвращает коллекцию Id проемов из активного документа, которые пересекаются с текущим заданием на отверстие из связи
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий из активного документа ревита</param>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="thisOpeningBBox">Бокс текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingOpeningsIds(ICollection<OpeningReal> realOpenings, Solid thisOpeningSolid, BoundingBoxXYZ thisOpeningBBox) {
            if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0)) {
                return Array.Empty<ElementId>();
            } else {
                return realOpenings.Where(realOpening => realOpening.IntersectsSolid(thisOpeningSolid, thisOpeningBBox)).Select(opening => new ElementId(opening.Id)).ToList();
            }
        }

        /// <summary>
        /// Возвращает Id основы экземпляра семейства задания на отверстие. 
        /// Под основой понимается либо элемент конструкции, с которым пересекается задание на отверстие, либо хост чистового отверстия, с которым пересекается задание на отверстие.
        /// </summary>
        /// <param name="intersectingStructureElements">Коллекция элементов конструкций из активного документа, с которыми пересекается задание на отверстие</param>
        /// <param name="intersectingOpenings">Коллекция чистовых отверстий из активного документа, с которыми пересекается задание на отверсите</param>
        /// <returns></returns>
        private ElementId GetOpeningTaskHostId(ICollection<ElementId> intersectingStructureElements, ICollection<ElementId> intersectingOpenings) {
            if((intersectingStructureElements != null) && intersectingStructureElements.Any()) {
                return intersectingStructureElements.First();
            } else if((intersectingOpenings != null) && intersectingOpenings.Any()) {
                return (_revitRepository.GetElement(intersectingOpenings.First()) as FamilyInstance)?.Host?.Id ?? ElementId.InvalidElementId;
            } else {
                return ElementId.InvalidElementId;
            }
        }

        /// <summary>
        /// Назначает название хоста задания на отверстие по Id
        /// </summary>
        /// <param name="hostId">Id хоста задания на отверстие. Под хостом понимается элемент конструкций из активного документа (стены/перекрытия), с которым пересекается текущее задание на отверстие</param>
        private void SetOpeningTaskHostName(ElementId hostId) {
            if(hostId != null) {
                var hostEl = _revitRepository.GetElement(hostId);
                if(hostEl != null) {
                    HostName = hostEl.Name;
                }
            }
        }
    }
}
