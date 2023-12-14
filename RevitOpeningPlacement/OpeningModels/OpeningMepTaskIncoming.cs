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
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия из связанного файла-задания на отверстия,
    /// подгруженного в текущий документ получателя
    /// </summary>
    internal class OpeningMepTaskIncoming : IOpeningTaskIncoming, IEquatable<OpeningMepTaskIncoming>, IFamilyInstanceProvider {
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
        /// <para>Примечание: конструктор не обновляет свойства <see cref="Status"/>, <see cref="HostName"/> и <see cref="Host"/>. Для обновления этих свойств надо вызвать <see cref="UpdateStatusAndHostName"/></para>
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

            Id = _familyInstance.Id;
            Transform = transform;
            Location = Transform.OfPoint((_familyInstance.Location as LocationPoint).Point);
            // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
            Rotation = (_familyInstance.Location as LocationPoint).Rotation + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);
            FileName = _familyInstance.Document.PathName;
            OpeningType = RevitRepository.GetOpeningType(openingTaskIncoming.Symbol.Family.Name);

            Date = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDate);
            MepSystem = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningMepSystem);
            Description = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDescription);
            CenterOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetCenter);
            BottomOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetBottom);
            DisplayDiameter = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDiameter);
            DisplayHeight = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningHeight);
            DisplayWidth = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningWidth);
            DisplayThickness = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningThickness);
            Comment = _familyInstance.GetParamValueStringOrDefault(
                SystemParamsConfig.Instance.CreateRevitParam(
                    _familyInstance.Document,
                    BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
                string.Empty);
            Username = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningAuthor);

            Diameter = GetFamilyInstanceDoubleParamValueOrZero(RevitRepository.OpeningDiameter);
            Height = GetFamilyInstanceDoubleParamValueOrZero(RevitRepository.OpeningHeight);
            Width = GetFamilyInstanceDoubleParamValueOrZero(RevitRepository.OpeningWidth);
            Thickness = GetFamilyInstanceDoubleParamValueOrZero(RevitRepository.OpeningThickness);

            string[] famNameParts = _familyInstance.Symbol.FamilyName.Split('_');
            if(famNameParts.Length > 0) {
                FamilyShortName = famNameParts.Last();
            }
        }


        public string FileName { get; }

        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public ElementId Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства входящего задания на отверстие в координатах активного документа - получателя заданий
        /// </summary>
        public XYZ Location { get; }

        public string Date { get; } = string.Empty;

        public string MepSystem { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public string CenterOffset { get; } = string.Empty;

        public string BottomOffset { get; } = string.Empty;

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
        /// Значение толщины в мм. Если толщины у отверстия нет, будет пустая строка.
        /// </summary>
        public string DisplayThickness { get; } = string.Empty;

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

        /// <summary>
        /// Толщина в единицах ревита или 0, если толщины нет
        /// </summary>
        public double Thickness { get; } = 0;

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
        /// <para>Для обновления использовать <see cref="UpdateStatusAndHostName"/></para>
        /// </summary>
        public string HostName { get; private set; } = string.Empty;

        /// <summary>
        /// Элемент из активного документа, в котором расположено задание на отверстие из связи.
        /// <para>Для обновления использовать <see cref="UpdateStatusAndHostName"/></para>
        /// </summary>
        public Element Host { get; private set; } = default;

        /// <summary>
        /// Комментарий экземпляра семейства задания на отверстие
        /// </summary>
        public string Comment { get; } = string.Empty;

        /// <summary>
        /// Имя пользователя, создавшего задание на отверстие
        /// </summary>
        public string Username { get; } = string.Empty;

        /// <summary>
        /// Статус отработки задания на отверстие
        /// <para>Для обновления использовать <see cref="UpdateStatusAndHostName"/></para>
        /// </summary>
        public OpeningTaskIncomingStatus Status { get; private set; } = OpeningTaskIncomingStatus.New;

        /// <summary>
        /// Тип проема
        /// </summary>
        public OpeningType OpeningType { get; } = OpeningType.WallRectangle;

        /// <summary>
        /// Угол поворота задания на отверстие в радианах в координатах активного файла, в который подгружена связь с заданием на отверстие
        /// </summary>
        public double Rotation { get; } = 0;

        public override bool Equals(object obj) {
            return (obj is OpeningMepTaskIncoming opening) && Equals(opening);
        }

        public override int GetHashCode() {
            return (int) Id.GetIdValue() + FileName.GetHashCode();
        }

        public bool Equals(OpeningMepTaskIncoming other) {
            return (other != null) && (Id == other.Id) && FileName.Equals(other.FileName);
        }


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

        /// <summary>
        /// Возвращает BBox в координатах активного документа-получателя заданий на отверстия, в который подгружены связи с заданиями
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            // _familyInstance.GetBoundingBox().TransformBoundingBox(Transform);
            // при получении бокса сразу из экземпляра семейства
            // сначала строится бокс в координатах экземпляра семейства,
            // а потом строится описанный бокс в координатах проекта.
            //      B = b*cos(a) + b*sin(a), где
            //      (a) - угол поворота
            //      (B) - сторона описанного бокса в координатах проекта
            //      (b) - сторона вписанного бокса в координатах экземпляра семейства
            // Если семейство - вертикальный цилиндр, то это приведет к значительной погрешности.
            return GetSolid().GetTransformedBoundingBox();
        }

        /// <summary>
        /// Обновляет <see cref="Status"/> и <see cref="HostName"/> входящего задания на отверстие
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий, размещенных в активном документе-получателе заданий на отверстия</param>
        /// <param name="constructureElementsIds">Коллекция элементов конструкций в активном документе-получателе заданий на отверстия</param>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateStatusAndHostName(ICollection<IOpeningReal> realOpenings, ICollection<ElementId> constructureElementsIds) {
            var thisOpeningSolid = GetSolid();
            var thisOpeningBBox = GetTransformedBBoxXYZ();

            var intersectingStructureElements = GetIntersectingStructureElementsIds(thisOpeningSolid, constructureElementsIds);
            var intersectingOpenings = GetIntersectingOpeningsIds(realOpenings, thisOpeningSolid, thisOpeningBBox);
            var hostId = GetOpeningTaskHostId(thisOpeningSolid, intersectingStructureElements, intersectingOpenings);
            SetOpeningTaskHost(hostId);

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
            //if(_familyInstance.IsExistsSharedParam(paramName)) {
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
        /// Возвращает коллекцию элементов конструкций, с которыми пересекается текущее задание на отверстие
        /// </summary>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="constructureElementsIds">Коллекция id элементов конструкций из активного документа ревита, для которых были сделаны задания на отверстия</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingStructureElementsIds(Solid thisOpeningSolid, ICollection<ElementId> constructureElementsIds) {
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
        /// Возвращает коллекцию Id проемов из активного документа, которые пересекаются с текущим заданием на отверстие из связи
        /// </summary>
        /// <param name="realOpenings">Коллекция чистовых отверстий из активного документа ревита</param>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <param name="thisOpeningBBox">Бокс текущего задания на отверстие в координатах активного файла - получателя заданий</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingOpeningsIds(ICollection<IOpeningReal> realOpenings, Solid thisOpeningSolid, BoundingBoxXYZ thisOpeningBBox) {
            if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0)) {
                return Array.Empty<ElementId>();
            } else {
                // для ускорения поиск первого пересечения
                var opening = realOpenings.FirstOrDefault(realOpening => realOpening.IntersectsSolid(thisOpeningSolid, thisOpeningBBox));
                if(opening != null) {
                    return new ElementId[] { opening.Id };
                } else {
                    return Array.Empty<ElementId>();
                }
            }
        }

        /// <summary>
        /// Возвращает Id элемента конструкции, который наиболее похож на хост для задания на отверстие.
        /// <para>Под наиболее подходящим понимается элемент конструкции, с которым пересечение наибольшего объема, либо хост чистового отверстия, с которым пересекается задание на отверстие.</para> 
        /// </summary>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие в координатах активного файла-получателя заданий</param>
        /// <param name="intersectingStructureElementsIds">Коллекция Id элементов конструкций из активного документа, с которыми пересекается задание на отверстие</param>
        /// <param name="intersectingOpeningsIds">Коллекция Id чистовых отверстий из активного документа, с которыми пересекается задание на отверсите</param>
        /// <returns></returns>
        private ElementId GetOpeningTaskHostId(Solid thisOpeningSolid, ICollection<ElementId> intersectingStructureElementsIds, ICollection<ElementId> intersectingOpeningsIds) {
            if((intersectingOpeningsIds != null) && intersectingOpeningsIds.Any()) {
                return (_revitRepository.GetElement(intersectingOpeningsIds.First()) as FamilyInstance)?.Host?.Id ?? ElementId.InvalidElementId;

            } else if((thisOpeningSolid != null) && (thisOpeningSolid.Volume > 0) && (intersectingStructureElementsIds != null) && intersectingStructureElementsIds.Any()) {

                // поиск элемента конструкции, с которым пересечение задания на отверстие имеет наибольший объем
                double halfOpeningVolume = thisOpeningSolid.Volume / 2;
                double intersectingVolumePrevious = 0;
                ElementId hostId = intersectingStructureElementsIds.First();
                foreach(var structureElementId in intersectingStructureElementsIds) {
                    var structureSolid = _revitRepository.GetElement(structureElementId)?.GetSolid();
                    if((structureSolid != null) && (structureSolid.Volume > 0)) {
                        try {
                            double intersectingVolumeCurrent = BooleanOperationsUtils.ExecuteBooleanOperation(thisOpeningSolid, structureSolid, BooleanOperationsType.Intersect)?.Volume ?? 0;
                            if(intersectingVolumeCurrent >= halfOpeningVolume) {
                                return structureElementId;
                            }
                            if(intersectingVolumeCurrent > intersectingVolumePrevious) {
                                intersectingVolumePrevious = intersectingVolumeCurrent;
                                hostId = structureElementId;
                            }
                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            continue;
                        }
                    }
                }
                return hostId;
            } else {
                return ElementId.InvalidElementId;
            }
        }

        /// <summary>
        /// Записывает элемент и название хоста задания на отверстие соответственно в свойства <see cref="Host"/> и <see cref="HostName"/>
        /// </summary>
        /// <param name="hostId">Id хоста задания на отверстие из активного документа (стены/перекрытия)</param>
        private void SetOpeningTaskHost(ElementId hostId) {
            if(hostId != null) {
                Host = _revitRepository.GetElement(hostId);
                var name = Host?.Name;
                if(!string.IsNullOrWhiteSpace(name)) {
                    HostName = name;
                }
            }
        }
    }
}
