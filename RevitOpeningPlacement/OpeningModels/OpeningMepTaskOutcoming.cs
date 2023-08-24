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

        /// <summary>
        /// Допустимый объем, равный кубу <see cref="_distance3dTolerance"/>
        /// </summary>
        private static readonly double _volumeTolerance = _distance3dTolerance * _distance3dTolerance * _distance3dTolerance;

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
        /// Флаг, обозначающий статус исходящего задания на отверстие
        /// <para>Для обновления использовать <see cref="UpdateStatus"/></para>
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
                if((openingSolid is null) || (openingSolid.Volume < _volumeTolerance)) {
                    Status = OpeningTaskOutcomingStatus.Invalid;
                    return;
                }

                if(ThisOpeningTaskIsNotActual(openingSolid, constructureLinkElementsProviders, allMepElementsIds)) {
                    Status = OpeningTaskOutcomingStatus.NotActual;
                    return;
                }

                var intersectingOpeningIds = GetIntersectingOpeningsTasks(openingSolid, openingsOutcomingTasksIdsForChecking);
                if(intersectingOpeningIds.Count > 0) {
                    Status = OpeningTaskOutcomingStatus.Intersects;
                    return;
                } else {
                    openingsOutcomingTasksIdsForChecking.Remove(new ElementId(Id));
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
                return openingTasks.Intersect(intersects, _equalityComparer).ToHashSet();
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
            return openingTasks.Where(task => !task.IsRemoved).Select(task => new ElementId(task.Id)).ToHashSet();
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
        private ICollection<Solid> GetIntersectingMepSolids(Solid thisOpeningTaskSolid, ICollection<ElementId> allMepElementsIds) {
            if((thisOpeningTaskSolid is null) || (thisOpeningTaskSolid.Volume <= 0)) {
                return Array.Empty<Solid>();
            } else {
                return GetIntersectingMepElementsIds(thisOpeningTaskSolid, allMepElementsIds)
                    .Select(elementId => GetDocument().GetElement(elementId).GetSolid())
                    .ToHashSet();
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
        /// <returns>True - задание точно некорректно; False - задание не некорректно, но утверждать, что оно корректно нельзя</returns>
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

            var mepElementsIntersectingThisTask = GetIntersectingMepElementsIds(thisOpeningTaskSolid, allMepElementsIds);
            if(mepElementsIntersectingThisTask.Count == 0) {
                // задание на отверстие не пересекается ни с одним элементом инженерной системы - задание не актуально
                return true;
            }

            // проверка на то, что:
            // во-первых есть конструкции в связанных файлах, внутри которых расположено задание на отверстие,
            // во-вторых, что ни один элемент ВИС, проходящий через текущее задание на отверстие не пересекается с этими конструкциями
            foreach(var link in constructureLinkElementsProviders) {
                var hostConstructions = GetHostConstructionsForThisOpeningTask(thisOpeningTaskSolid, link, out ICollection<OpeningReal> intersectingOpenings);
                if(hostConstructions.Count > 0) {
                    return MepElementsIntersectConstructionsOrOpenings(
                        thisOpeningTaskSolid,
                        mepElementsIntersectingThisTask,
                        hostConstructions,
                        intersectingOpenings,
                        link);
                } else {
                    // если не найдены конструкции, которые можно считать хостами текущего задания на отверстие,
                    // то либо задание на отверстие висит в воздухе, либо задание на отверстие пересекается с другой связью
                    continue;
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
        /// <param name="intersectingOpeningsReal">Чистовые отверстия из связи, которые пересекаются с солидом текущего задания на отверстие</param>
        /// <returns>
        /// Коллекция Id конструкций (стен или перекрытий) из связанного файла, которые пересекаются с солидом текущего задания на отверстие
        /// <para>или коллекция Id конструкций, которые являются хостами чистовых отверстий, с которыми пересекается солид текущего задания на отверстие</para> 
        /// </returns>
        private ICollection<ElementId> GetHostConstructionsForThisOpeningTask(Solid thisOpeningTaskSolid, IConstructureLinkElementsProvider link, out ICollection<OpeningReal> intersectingOpeningsReal) {
            var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);

            intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, thisOpeningTaskSolid);

            // поиск конструкций из связи, в которых находится текущее задание на отверстие
            var intersectingConstructions = GetIntersectingLinkConstructionElementsIds(thisSolidInLinkCoordinates, link);
            if(intersectingConstructions.Count == 0) {
                // задание на отверстие не пересекается с конструкциями из связей

                // поиск чистовых отверстий из связи, которые пересекаются с текущим заданием на отверстие
                if(intersectingOpeningsReal.Count > 0) {
                    // пересечение с чистовыми отверстиями из связей найдено

                    // поиск элементов конструкций - основ чистовых отверстий из связей
                    intersectingConstructions = intersectingOpeningsReal.Select(opening => opening.GetHost().Id).Distinct().ToHashSet();
                }
            } else {
                // если задание на отверстие пересекается с конструкциями из связей, то будем считать, что даже если задание также пересекается с чистовыми отверстиями,
                // то хосты этих чистовых отверстий - это и есть конструкции, с которым пересекается само задание на отверстие.
                // Поэтому можно не делать долгую проверку на поиск пересекающих чистовых отверстий (см. ниже).

                // Если эта логика окажется неверной, то надо убрать комментарии ниже
                //intersectingOpeningsReal = GetIntersectingLinkOpeningsReal(link, thisOpeningTaskSolid);
                //if(intersectingOpeningsReal.Count > 0) {

                //    var openingsHosts = intersectingOpeningsReal.Select(opening => opening.GetHost().Id).Distinct().ToHashSet();
                //    var constructions = new List<ElementId>(intersectingConstructions);
                //    constructions.AddRange(openingsHosts);
                //    intersectingConstructions = constructions;
                //}
            }

            return intersectingConstructions;
        }

        /// <summary>
        /// Поиск чистовых отверстий из связанного файла, в которых расположено задание на отверстие
        /// </summary>
        /// <param name="link">Связь с конструкциями и чистовыми отверстиями</param>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <returns>Коллекция чистовых отверстий из связи, которые пересекаются с солидом текущего задания на отверстие</returns>
        private ICollection<OpeningReal> GetIntersectingLinkOpeningsReal(IConstructureLinkElementsProvider link, Solid thisOpeningTaskSolid) {
            var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);
            var thisBBoxInLinkCoordinates = GetTransformedBBoxXYZ().TransformBoundingBox(link.DocumentTransform.Inverse);
            return link.GetOpeningsReal().Where(realOpening => realOpening.IntersectsSolid(thisSolidInLinkCoordinates, thisBBoxInLinkCoordinates)).ToHashSet();
        }

        /// <summary>
        /// Проверяет, пересекаются ли элементы ВИС из текущего файла с конструкциями или чистовыми отверстиями, причем место пересечения находится вне солида задания на отверстие.
        /// <para>При этом проверяемые элементы ВИС, а также проверяемые конструкции и чистовые отверстия из связи должны пересекать солид текущего задания на отверстие</para>
        /// </summary>
        /// <param name="thisOpeningTaskSolid">Солид текущего задания на отверстие</param>
        /// <param name="intersectingMepElementsIds">Элементы ВИС из текущего файла с заданиями на отверстия, которые пересекают солид текущего задания на отверстие</param>
        /// <param name="intersectingLinkConstructions">Конструкции из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="intersectingLinkOpeningsReal">Чистовые отверстия из <paramref name="link"/>, которые пересекаются с текущим заданием на отверстие</param>
        /// <param name="link">Связанный файл с конструкциями</param>
        /// <returns>True, если найдено пересечение элементов ВИС и конструкций (или чистовых отверстий) вне солида задания на отверстие, иначе False</returns>
        private bool MepElementsIntersectConstructionsOrOpenings(
            Solid thisOpeningTaskSolid,
            ICollection<ElementId> intersectingMepElementsIds,
            ICollection<ElementId> intersectingLinkConstructions,
            ICollection<OpeningReal> intersectingLinkOpeningsReal,
            IConstructureLinkElementsProvider link
            ) {
            if(!IsRemoved
                && (link != null)
                && (intersectingMepElementsIds != null)
                && (intersectingMepElementsIds.Count > 0)
                && (intersectingLinkConstructions != null)
                && (intersectingLinkConstructions.Count > 0)
                && (intersectingLinkOpeningsReal != null)
                && (thisOpeningTaskSolid != null)
                && (thisOpeningTaskSolid.Volume > 0)) {

                var thisSolidInLinkCoordinates = SolidUtils.CreateTransformed(thisOpeningTaskSolid, link.DocumentTransform.Inverse);

                // получение объединенного солида для элементов ВИС в координатах текущего файла с заданиями на отверстия
                var mepElements = intersectingMepElementsIds.Select(mepId => GetDocument().GetElement(mepId)).ToHashSet();
                var mepSolids = mepElements.Select(el => el.GetSolid()).Where(solid => (solid != null) && (solid.Volume > 0)).ToList();
                var mepUnitedSolid = RevitClashDetective.Models.Extensions.ElementExtensions.UniteSolids(mepSolids);
                if((mepUnitedSolid is null) || mepUnitedSolid.Volume < _volumeTolerance) {
                    return false;
                }

                try {
                    // трансформация объединенного солида элементов ВИС в координаты связанного файла с конструкциями
                    var mepSolidInLinkCoordinates = SolidUtils.CreateTransformed(mepUnitedSolid, link.DocumentTransform.Inverse);
                    // вычитание из объединенного солида элементов ВИС солида задания на отверстие,
                    // чтобы исключить из проверки места пересечения элементов ВИС с конструкциями внутри тела солида задания на отверстие
                    var mepSolidMinusOpeningTask = BooleanOperationsUtils.ExecuteBooleanOperation(mepSolidInLinkCoordinates, thisSolidInLinkCoordinates, BooleanOperationsType.Difference);

                    // поиск конструкций (стен и перекрытий) и чистовых отверстий из связанного файла, которые пересекаются с проходящими через задание на отверстие элементами ВИС из текущего файла,
                    // с учетом того, что место пересечения находится вне тела задания на отверстие.
                    var mepElementsSolid = mepElements.Select(el => el.GetBoundingBox()).GetCommonBoundingBox().TransformBoundingBox(link.DocumentTransform.Inverse);
                    return new FilteredElementCollector(link.Document, intersectingLinkConstructions)
                        .WherePasses(new ElementIntersectsSolidFilter(mepSolidMinusOpeningTask))
                        .Any()
                        || intersectingLinkOpeningsReal.Any(openingReal => openingReal.IntersectsSolid(mepSolidMinusOpeningTask, mepElementsSolid));

                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}
