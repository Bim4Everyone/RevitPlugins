﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Comparers;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия, 
    /// размещаемые в файлах-источниках заданий на отверстия для последующей передачи этих заданий получателю
    /// </summary>
    internal class OpeningMepTaskOutcoming : ISolidProvider, IEquatable<OpeningMepTaskOutcoming> {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly FamilyInstance _familyInstance;

        /// <summary>
        /// Допустимое расстояние между экземплярами семейств заданий на отверстия, при котором считается, что они размещены в одном и том же месте
        /// </summary>
        private static readonly double _distance3dTolerance = Math.Sqrt(3 * XYZExtension.FeetRound * XYZExtension.FeetRound);

        private static readonly OpeningTaskOutcomingEqualityComparer _equalityComparer = new OpeningTaskOutcomingEqualityComparer();

        /// <summary>
        /// Кэш для хранения результата метода <see cref="GetIntersectingMepElementsIds"/>
        /// </summary>
        private (ICollection<ElementId> Value, DateTime CacheTime) _intersectingMepElementsCache;

        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningMepTaskOutcoming"/>
        /// <para>Примечание: конструктор не обновляет свойство <see cref="Status"/>. Для обновления этого свойства нужно вызвать <see cref="UpdateStatus"/></para>
        /// </summary>
        /// <param name="openingTaskOutcoming">Экземпляр семейства задания на отверстие, расположенного в текущем документе Revit</param>
        public OpeningMepTaskOutcoming(FamilyInstance openingTaskOutcoming) {
            _familyInstance = openingTaskOutcoming;
            Id = _familyInstance.Id.IntegerValue;
            Location = (_familyInstance.Location as LocationPoint).Point;

            Date = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDate);
            MepSystem = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningMepSystem);
            Description = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDescription);
            CenterOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetCenter);
            BottomOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetBottom);
        }

        /// <summary>
        /// Дата создания
        /// </summary>
        public string Date { get; } = string.Empty;

        /// <summary>
        /// Название инженерной системы, для элемента которой создано задание на отверстие
        /// </summary>
        public string MepSystem { get; } = string.Empty;

        /// <summary>
        /// Описание задания на отверстие
        /// </summary>
        public string Description { get; } = string.Empty;

        /// <summary>
        /// Отметка центра
        /// </summary>
        public string CenterOffset { get; } = string.Empty;

        /// <summary>
        /// Отметка низа
        /// </summary>
        public string BottomOffset { get; } = string.Empty;

        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; private set; }

        /// <summary>
        /// Флаг, обозначающий, удален ли экземпляр семейства задания на отверстие из проекта
        /// </summary>
        public bool IsRemoved => (_familyInstance is null) || (!_familyInstance.IsValidObject);

        /// <summary>
        /// Флаг, обозначающий статус задания на отверстие
        /// 
        /// Например, в файле инженерных систем, 
        /// если экземпляр семейства задания не пересекается ни с одним элементом инженерных систем, то NoBasis
        /// если экземпляр семейства задания пересекается с каким-то элементом инженерных систем и это пересечение некорректно, то InaccurateBasis
        /// если экземпляр семейства задания пересекается с каким-то элементом инженерных систем и это пересечение корректно, то HasBasis
        /// </summary>
        public OpeningTaskOutcomingStatus Status { get; set; } = OpeningTaskOutcomingStatus.NotActual;


        /// <summary>
        /// Возвращает экземпляр семейства задания на отверстие
        /// </summary>
        /// <returns></returns>
        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }

        /// <summary>
        /// <para>Обновляет <see cref="Status"/> задания на отверстие.</para>
        /// 
        /// Обновление происходит по соотношению объема пересекаемого Solid задания на отверстие элементами инженерных систем из файла задания на отверстие и исходным Solid этого задания на отверстие.
        /// Если соотношение объемов >= 0.95, то статус <see cref="OpeningTaskOutcomingStatus.TooSmall"/>,
        /// Если соотношение объемов в диапазоне [0.2, 0.95), то статус <see cref="OpeningTaskOutcomingStatus.Correct"/>,
        /// Если соотношение объемов в диапазоне (0; 0.2), то статус <see cref="OpeningTaskOutcomingStatus.TooBig"/>,
        /// Если соотношение объемов равно 0, то статус <see cref="OpeningTaskOutcomingStatus.NotActual"/>.
        /// 
        /// <para>Если же объем Solid самого задания на отверстие равен 0, то статус <see cref="OpeningTaskOutcomingStatus.Invalid"/></para>
        /// </summary>
        /// <param name="openingsOutcomingTasksIdsForChecking">Коллекция Id экземпляров семейств заданий на отверстия из активного файла для проверки</param>
        /// <param name="allMepElementsIds">Коллекция Id всех элементов инженерных систем из активного файла</param>
        public void UpdateStatus(
            ref IList<ElementId> openingsOutcomingTasksIdsForChecking,
            ICollection<ElementId> allMepElementsIds,
            ICollection<IConstructureLinkElementsProvider> constructureLinkElementsProviders) {

            if(openingsOutcomingTasksIdsForChecking is null) {
                throw new ArgumentNullException(nameof(openingsOutcomingTasksIdsForChecking));
            }
            if(allMepElementsIds is null) {
                throw new ArgumentNullException(nameof(allMepElementsIds));
            }
            if(!IsRemoved) {
                var openingSolid = GetSolid();
                if((openingSolid != null) && (openingSolid.Volume > 0)) {
                    var intersectingOpeningIds = GetIntersectingOpeningsTasks(openingSolid, openingsOutcomingTasksIdsForChecking);
                    if(intersectingOpeningIds.Count > 0) {
                        Status = OpeningTaskOutcomingStatus.Intersects;
                        return;
                    } else {
                        openingsOutcomingTasksIdsForChecking.Remove(new ElementId(Id));
                    }

                    if(ThisOpeningTaskIsNotActual(openingSolid, constructureLinkElementsProviders, allMepElementsIds)) {
                        Status = OpeningTaskOutcomingStatus.NotActual;
                        return;
                    }
                    try {
                        Solid openingSolidAfterIntersection = GetOpeningAndMepsSolidsDifference(openingSolid, allMepElementsIds);
                        if(openingSolidAfterIntersection is null) {
                            Status = OpeningTaskOutcomingStatus.Invalid;
                            return;
                        }
                        double volumeRatio = (openingSolid.Volume - openingSolidAfterIntersection.Volume) / openingSolid.Volume;
                        Status = GetOpeningTaskOutcomingStatus(volumeRatio);

                    } catch(InvalidOperationException) {
                        Status = OpeningTaskOutcomingStatus.Invalid;
                        return;
                    }
                } else {
                    Status = OpeningTaskOutcomingStatus.Invalid;
                }
            }
        }

        /// <summary>
        /// Возвращает Solid экземпляра семейства задания на отверстие с трансформированными координатами, 
        /// если элемент был удален из документа, будет возвращено значение по умолчанию
        /// </summary>
        /// <returns></returns>
        public Solid GetSolid() {
            if(IsRemoved) {
                return default;
            }
            return _familyInstance.GetSolid();
        }

        /// <summary>
        /// Возвращает BoundingBoxXYZ с учетом расположения <see cref="_familyInstance">элемента</see> в файле Revit, 
        /// если элемент был удален из документа, будет возвращено значение по умолчанию
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            if(IsRemoved) {
                return default;
            }
            return _familyInstance.GetBoundingBox();
        }


        /// <summary>
        /// Проверяет, размещено ли уже такое же задание на отверстие в проекте. Под "таким" же понимается семейство задания на отверстие с координатами
        /// </summary>
        /// <param name="placedOpenings">Существующие задания на отверстия в проекте</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool IsAlreadyPlaced(ICollection<OpeningMepTaskOutcoming> placedOpenings) {
            if(IsRemoved || placedOpenings.Count == 0) {
                return false;
            }
            var closestPlacedOpenings = GetIntersectingTasksRaw(placedOpenings).Where(placedOpening =>
                placedOpening.Location
                .DistanceTo(placedOpening.Location) <= _distance3dTolerance);

            foreach(OpeningMepTaskOutcoming placedOpening in closestPlacedOpenings) {
                if(placedOpening.EqualsSolid(GetSolid(), XYZExtension.FeetRound)) {
                    return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj) {
            if(obj is null) {
                return false;
            } else if(obj is OpeningMepTaskOutcoming openingOther) {
                return (Id == openingOther.Id) && GetDocument().Equals(openingOther.GetDocument());
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public bool Equals(OpeningMepTaskOutcoming other) {
            return (Id == other.Id) && GetDocument().Equals(other.GetDocument());
        }

        /// <summary>
        /// Первоначальная быстрая проверка поданной коллекции заданий на отверстия из текущего файла на пересечение с текущим заданием на отверстие
        /// </summary>
        /// <param name="openingTasks"></param>
        /// <returns></returns>
        private ICollection<OpeningMepTaskOutcoming> GetIntersectingTasksRaw(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            if(!IsRemoved) {
                var intersects = new FilteredElementCollector(GetDocument(), GetTasksIds(openingTasks))
                .Excluding(new ElementId[] { new ElementId(Id) })
                .WherePasses(GetBoundingBoxFilter())
                .Cast<FamilyInstance>()
                .Select(famInst => new OpeningMepTaskOutcoming(famInst));
                return openingTasks.Intersect(intersects, _equalityComparer).ToList();
            } else {
                return Array.Empty<OpeningMepTaskOutcoming>();
            }
        }

        private Outline GetOutline() {
            if(IsRemoved) {
                return default;
            }
            var bb = GetTransformedBBoxXYZ();
            return new Outline(bb.Min, bb.Max);
        }

        private BoundingBoxIntersectsFilter GetBoundingBoxFilter() {
            if(IsRemoved) {
                return default;
            }
            return new BoundingBoxIntersectsFilter(GetOutline());
        }

        /// <summary>
        /// Возвращает документ, в котором размещено семейство отверстия. 
        /// Если экземпляр семейства удален, будет выброшено исключение <see cref="Autodesk.Revit.Exceptions.InvalidObjectException"/>
        /// Проверку на удаление делать через <see cref="IsRemoved"/>
        /// </summary>
        /// <returns></returns>
        private Document GetDocument() {
            if(IsRemoved) {
                return default;
            }
            return _familyInstance.Document;
        }

        private ICollection<ElementId> GetTasksIds(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            return openingTasks.Where(task => !task.IsRemoved).Select(task => new ElementId(task.Id)).ToList();
        }

        /// <summary>
        /// Возвращает строковое значение параметра по названию или пустую строку, если параметр отсутствует у текущего экземпляра семейства задания на отверстие
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
                object paramValue = _familyInstance.GetParamValue(paramName);
                if(!(paramValue is null)) {
                    value = paramValue.ToString();
                }
            }
            return value;
        }

        /// <summary>
        /// Возвращает статус задания на отверстие по отношению объема пересечения задания на отверстие с элементом инженерной системы к исходному объему задания на отверстие
        /// </summary>
        /// <param name="volumeRatio"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Исключение, если коэффициент отношения объемов меньше 0 или больше 1</exception>
        private OpeningTaskOutcomingStatus GetOpeningTaskOutcomingStatus(double volumeRatio) {
            if((volumeRatio < 0) || (volumeRatio > 1)) {
                throw new ArgumentOutOfRangeException($"Значение параметра {nameof(volumeRatio)} должно находиться в интервале [0; 1]");
            }
            OpeningTaskOutcomingStatus status;
            if(0.95 <= volumeRatio) {
                status = OpeningTaskOutcomingStatus.TooSmall;
            } else if((0.2 <= volumeRatio) && (volumeRatio < 0.95)) {
                status = OpeningTaskOutcomingStatus.Correct;
            } else if((0.01 < volumeRatio) && (volumeRatio < 0.2)) {
                status = OpeningTaskOutcomingStatus.TooBig;
            } else {
                status = OpeningTaskOutcomingStatus.NotActual;
            }
            return status;
        }

        /// <summary>
        /// Возвращает Id всех исходящих заданий на отверстия, которые пересекаются с данным заданием на отверстие из текущего документа
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="allOpeningTasksInDoc">Id всех экземпляров семейств заданий на отверстия из активного документа ревита для проверки</param>
        /// <returns>Коллекция Id экземпляров семейств исходящих заданий на отверстия, которые пересекаются с текущим заданием на отверстие</returns>
        private ICollection<ElementId> GetIntersectingOpeningsTasks(Solid thisOpeningTaskSolid, ICollection<ElementId> allOpeningTasksInDoc) {
            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                return Array.Empty<ElementId>();
            } else {
                return new FilteredElementCollector(GetDocument(), allOpeningTasksInDoc)
                    .Excluding(new ElementId[] { new ElementId(Id) })
                    .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningTaskSolid.GetOutline()))
                    .WherePasses(new ElementIntersectsSolidFilter(thisOpeningTaskSolid))
                    .ToElementIds();
            }
        }


        /// <summary>
        /// Возвращает список солидов элементов инженерных систем, которые пересекаются с данным заданием на отверстие, из текущего документа
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="allMepElementsIds">Коллекция Id всех элементов инженерных систем из файла, в котором размещено задание на отверстие</param>
        /// <returns></returns>
        private IList<Solid> GetIntersectingMepSolids(Solid thisOpeningTaskSolid, ICollection<ElementId> allMepElementsIds) {
            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                return new List<Solid>();
            } else {
                return GetIntersectingMepElementsIds(thisOpeningTaskSolid, allMepElementsIds)
                    .Select(elementId => GetDocument().GetElement(elementId).GetSolid())
                    .ToList();
            }
        }

        /// <summary>
        /// Возвращает список Id элементов инженерных систем, которые пересекаются с данным заданием на отверстие, из текущего документа
        /// </summary>
        /// <param name="thisOpeningSolid"></param>
        /// <param name="allMepElementsIds"></param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingMepElementsIds(Solid thisOpeningSolid, ICollection<ElementId> allMepElementsIds) {
            if(!IsRemoved && (thisOpeningSolid != null) && (allMepElementsIds != null)) {
                if((_intersectingMepElementsCache.Value != null) && (_intersectingMepElementsCache.CacheTime.CompareTo(DateTime.Now) >= 0)) {
                    return _intersectingMepElementsCache.Value;
                } else {
                    _intersectingMepElementsCache.Value = new FilteredElementCollector(GetDocument(), allMepElementsIds)
                        .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                        .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                        .ToElementIds();
                    _intersectingMepElementsCache.CacheTime = DateTime.Now.AddSeconds(1);
                    return _intersectingMepElementsCache.Value;
                }
            } else {
                return Array.Empty<ElementId>();
            }
        }

        /// <summary>
        /// Возвращает солид, образованный вычитанием из исходного солида задания на отверстие всех пересекающих его элементов инженерных систем
        /// </summary>
        /// <param name="thisOpeningSolid">Солид текущего задания на отверстие</param>
        /// <param name="allMepElementsIds">Коллекция Id всех элементов инженерных систем из файла, в котором размещено задание на отверстие</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Исключение, если не удалось выполнить вычитание солида отверстия и солида элемента инженерной системы</exception>
        private Solid GetOpeningAndMepsSolidsDifference(Solid thisOpeningSolid, ICollection<ElementId> allMepElementsIds) {
            var intersectingMepSolids = GetIntersectingMepSolids(thisOpeningSolid, allMepElementsIds);
            Solid openingSolidAfterIntersection = thisOpeningSolid;
            foreach(var mepSolid in intersectingMepSolids) {
                //здесь определяется солид, образованный вычитанием из исходного солида задания на отверстие всех пересекающих его элементов инженерных систем
                try {
                    openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(openingSolidAfterIntersection, mepSolid, BooleanOperationsType.Difference);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    try {
                        // если один солид касается другого солида изнутри, то будет исключение InvalidOperationException
                        // здесь производится попытка слегка подвинуть один из солидов, чтобы избежать этого касания
                        double coordinateOffset = 0.001;
                        var mepTransformedSolid = SolidUtils.CreateTransformed(mepSolid, Transform.CreateTranslation(new XYZ(coordinateOffset, coordinateOffset, coordinateOffset)));
                        openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(openingSolidAfterIntersection, mepTransformedSolid, BooleanOperationsType.Difference);

                    } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                        try {
                            // здесь производится попытка слегка подвинуть один из солидов в другую сторону
                            double coordinateOffset = -0.001;
                            var mepTransformedSolid = SolidUtils.CreateTransformed(mepSolid, Transform.CreateTranslation(new XYZ(coordinateOffset, coordinateOffset, coordinateOffset)));
                            openingSolidAfterIntersection = BooleanOperationsUtils.ExecuteBooleanOperation(openingSolidAfterIntersection, mepTransformedSolid, BooleanOperationsType.Difference);

                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            return openingSolidAfterIntersection;
        }

        /// <summary>
        /// Проверяет, является ли данное задание на отверстие НЕ актуальным. Если задание не актуально, возвращается False, иначе True.
        /// Метод может установить только НЕкорректность задания, но корректность абсолютно точно подтвердить не может.
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="constructureLinkElementsProviders">Коллекция объектов-оберток связанных файлов с элементами конструкций: стенами, перекрытиями и т.п.</param>
        /// <returns>True - задание некорректно; False - задание не некорректно, но утверждать, что оно корректно нельзя</returns>
        private bool ThisOpeningTaskIsNotActual(
            Solid thisOpeningTaskSolid,
            ICollection<IConstructureLinkElementsProvider> constructureLinkElementsProviders,
            ICollection<ElementId> allMepElementsIds
            ) {

            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                // геометрия задания на отверстие не корректна - задание не актуально
                return true;
            }
            if(constructureLinkElementsProviders is null) { throw new ArgumentNullException(nameof(constructureLinkElementsProviders)); }
            if(allMepElementsIds is null) { throw new ArgumentNullException(nameof(allMepElementsIds)); }

            var intersectingTaskMepElements = GetIntersectingMepElementsIds(thisOpeningTaskSolid, allMepElementsIds);
            if(intersectingTaskMepElements.Count == 0) {
                // задание на отверстие не пересекается ни с одним элементом инженерной системы - задание не актуально
                return true;
            }

            // проверка на то, что:
            // во-первых есть конструкции в связанных, внутри которых расположено задание на отверстие,
            // во-вторых, что ни один элемент ВИС, проходящий через текущее задание на отверстие не пересекается с этими конструкциями
            foreach(var link in constructureLinkElementsProviders) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(thisOpeningTaskSolid, link);
                if(hostConstructions.Count > 0) {
                    return MepElementsIntersectConstructions(thisOpeningTaskSolid, intersectingTaskMepElements, hostConstructions, link);
                }
            }
            // корректная ситуация не найдена, отверстие считается не актуальным
            return true;
        }

        /// <summary>
        /// Возвращает коллекцию Id элементов конструкций из <see cref="IConstructureLinkElementsProvider">связанного файла</see>, которые пересекаются с текущим заданием на отверстие
        /// </summary>
        /// <param name="thisOpeningSolidInLinkCoordinates">Солид текущего задания на отверстие в координатах связанного файла <paramref name="constructureLinkElementsProvider"/></param>
        /// <param name="constructureLinkElementsProvider">Связанный файл для проверки на пересечение</param>
        /// <returns></returns>
        private ICollection<ElementId> GetIntersectingLinkConstructionElementsIds(Solid thisOpeningSolidInLinkCoordinates, IConstructureLinkElementsProvider constructureLinkElementsProvider) {
            return new FilteredElementCollector(constructureLinkElementsProvider.Document, constructureLinkElementsProvider.GetConstructureElementIds())
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolidInLinkCoordinates.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolidInLinkCoordinates))
                .ToElementIds();
        }

        /// <summary>
        /// Поиск конструкций из связанного файла, в которых расположено задание на отверстие
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="link">Связь с конструкциями</param>
        /// <returns></returns>
        private ICollection<ElementId> GetHostConstructionsForThisOpeningTask(Solid thisOpeningTaskSolid, IConstructureLinkElementsProvider link) {
            var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);

            // поиск конструкций из связи, в которых находится текущее задание на отверстие
            var intersectingConstructions = GetIntersectingLinkConstructionElementsIds(thisSolidInLinkCoordinates, link);
            if(intersectingConstructions.Count == 0) {
                // задание на отверстие не пересекается с конструкциями из связей

                // поиск чистовых отверстий из связи, которые пересекаются с текущим заданием на отверстие
                var thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ().TransformBoundingBox(link.DocumentTransform.Inverse);
                var intersectingOpenings = link.GetOpeningsReal().Where(realOpening => realOpening.IntersectsSolid(thisSolidInLinkCoordinates, thisBBoxInLinkCoordinates)).ToList();
                if(intersectingOpenings.Count > 0) {
                    // пересечение с чистовыми отверстиями из связей найдено

                    // поиск элементов конструкций - основ чистовых отверстий из связей
                    intersectingConstructions = intersectingOpenings.Select(opening => opening.GetHost().Id).Distinct().ToList();
                }
            } else {
                // если задание на отверстие пересекается с конструкциями из связей, то будем считать, что даже если задание также пересекается с чистовыми отверстиями,
                // то хосты этих чистовых отверстий - это и есть конструкции, с которым пересекается само задание на отверстие.
                // Поэтому можно не делать долгую проверку на поиск пересекающих чистовых отверстий.

                // Если эта логика окажется неверной, то сюда надо убрать комментарии ниже
                //var thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ().TransformBoundingBox(link.DocumentTransform.Inverse);
                //var intersectingOpenings = link.GetOpeningsReal().Where(realOpening => realOpening.IntersectsSolid(thisSolidInLinkCoordinates, thisBBoxInLinkCoordinates)).ToList();
                //if(intersectingOpenings.Count > 0) {

                //    var openingsHosts = intersectingOpenings.Select(opening => opening.GetHost().Id).Distinct().ToList();
                //    var constructions = new List<ElementId>(intersectingConstructions);
                //    constructions.AddRange(openingsHosts);
                //    intersectingConstructions = constructions;
                //}
            }

            return intersectingConstructions;
        }

        /// <summary>
        /// Проверяет, пересекаются ли элементы ВИС с конструкциями, в которых находится текущее задание на отверстие
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего отверстия</param>
        /// <param name="intersectingMepElementsIds">Элементы ВИС из текущего файла с заданиями на отверстия, которые пересекают солид текущего задания на отверстие</param>
        /// <param name="constructionLinkElementsIds">Конструкции из связей, в которых расположено текущее задание на отверстие</param>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <returns></returns>
        private bool MepElementsIntersectConstructions(
            Solid thisOpeningTaskSolid,
            ICollection<ElementId> intersectingMepElementsIds,
            ICollection<ElementId> constructionLinkElementsIds,
            IConstructureLinkElementsProvider link
            ) {
            if(!IsRemoved && (intersectingMepElementsIds != null) && (constructionLinkElementsIds != null) && (link != null) && (thisOpeningTaskSolid != null) && (thisOpeningTaskSolid.Volume > 0)) {
                var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);

                // получение объединенного солида для элементов ВИС в координатах текущего файла с заданиями на отверстия
                var mepSolids = intersectingMepElementsIds.Select(mepId => GetDocument().GetElement(mepId).GetSolid()).Where(solid => (solid != null) && (solid.Volume > 0)).ToList();
                var mepUnitedSolid = RevitClashDetective.Models.Extensions.ElementExtensions.UniteSolids(mepSolids);
                if((mepUnitedSolid is null) || mepUnitedSolid.Volume < (_distance3dTolerance * _distance3dTolerance * _distance3dTolerance)) {
                    return false;
                }

                try {
                    // трансформация объединенного солида элементов ВИС в координаты связанного файла с конструкциями
                    var mepSolidInLinkCoordinates = SolidUtils.CreateTransformed(mepUnitedSolid, link.DocumentTransform.Inverse);
                    // вычитание из объединенного солида элементов ВИС солида задания на отверстие,
                    // чтобы исключить из проверки места пересечения элементов ВИС с конструкциями внутри тела солида задания на отверстие
                    var mepSolidMinusOpeningTask = BooleanOperationsUtils.ExecuteBooleanOperation(mepSolidInLinkCoordinates, thisSolidInLinkCoordinates, BooleanOperationsType.Difference);

                    // поиск конструкций из связанного файла, которые пересекаются с проходящими через задание на отверстие элементами ВИС из текущего файла,
                    // с учетом того, что место пересечения находится вне тела задания на отверстие
                    return new FilteredElementCollector(link.Document, constructionLinkElementsIds)
                        .WherePasses(new ElementIntersectsSolidFilter(mepSolidMinusOpeningTask))
                        .Any();

                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}