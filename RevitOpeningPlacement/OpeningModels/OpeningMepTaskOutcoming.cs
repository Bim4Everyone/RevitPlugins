using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

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
        private static readonly double _distance3dTolerance = Math.Sqrt(XYZExtension.FeetRound * XYZExtension.FeetRound * XYZExtension.FeetRound);

        private static readonly OpeningTaskOutcomingEqualityComparer _equalityComparer = new OpeningTaskOutcomingEqualityComparer();


        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningMepTaskOutcoming"/>
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

            UpdateStatus();
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
        public OpeningTaskOutcomingStatus Status { get; set; } = OpeningTaskOutcomingStatus.Empty;


        /// <summary>
        /// Возвращает экземпляр семейства задания на отверстие
        /// </summary>
        /// <returns></returns>
        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }

        /// <summary>
        /// Обновляет <see cref="Status"/> задания на отверстие.
        /// 
        /// Обновление происходит по соотношению объема пересекаемого Solid задания на отверстие элементами инженерных систем из файла задания на отверстие и исходным Solid этого задания на отверстие.
        /// Если соотношение объемов >= 0.95, то статус <see cref="OpeningTaskOutcomingStatus.TooSmall"/>,
        /// Если соотношение объемов в диапазоне [0.2, 0.95), то статус <see cref="OpeningTaskOutcomingStatus.Correct"/>,
        /// Если соотношение объемов в диапазоне (0; 0.2), то статус <see cref="OpeningTaskOutcomingStatus.TooBig"/>,
        /// Если соотношение объемов равно 0, то статус <see cref="OpeningTaskOutcomingStatus.Empty"/>.
        /// 
        /// Если же объем Solid самого задания на отверстие равен 0, то статус <see cref="OpeningTaskOutcomingStatus.Invalid"/>
        /// </summary>
        /// <param name="revitRepository"></param>
        public void UpdateStatus() {
            if(!IsRemoved) {
                var openingSolid = GetSolid();
                if((openingSolid != null) && (openingSolid.Volume > 0)) {
                    double openingVolume = openingSolid.Volume;
                    var t = new FilteredElementCollector(GetDocument())
                        .WherePasses(FiltersInitializer.GetFilterByAllUsedMepCategories())
                        .WherePasses(new BoundingBoxIntersectsFilter(openingSolid.GetOutline()))
                        .WherePasses(new ElementIntersectsSolidFilter(openingSolid));
                    //TODO убрать разделение linq
                    var intersectingMepSolids = t
                        .Select(element => element.GetSolid())
                        .ToList();
                    double intersectingMepVolume = 0;
                    if(Id == 6872355) {
                        var tryu = 0;
                    }
                    foreach(var mepSolid in intersectingMepSolids) {
                        Solid intersectedSolid;
                        try {
                            //TODO вычитать солид MEP из солида, полученного в предыдущей итерации цикла сейчас объем для нескольких пересечений считается завышенным
                            intersectedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(openingSolid, mepSolid, BooleanOperationsType.Intersect);
                        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                            try {
                                // если один солид касается другого солида изнутри, то будет исключение InvalidOperationException
                                // здесь производится попытка слегка подвинуть один из солидов, чтобы избежать этого касания
                                double coordinateOffset = 0.001;
                                var mepTransformedSolid = SolidUtils.CreateTransformed(mepSolid, Transform.CreateTranslation(new XYZ(coordinateOffset, coordinateOffset, coordinateOffset)));
                                intersectedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(openingSolid, mepTransformedSolid, BooleanOperationsType.Intersect);

                            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                                Status = OpeningTaskOutcomingStatus.Invalid;
                                return;
                            }
                        }
                        if(intersectedSolid != null) {
                            intersectingMepVolume += intersectedSolid.Volume;
                        }
                    }

                    double volumeRatio = intersectingMepVolume / openingVolume;
                    if(0.95 <= volumeRatio) {
                        Status = OpeningTaskOutcomingStatus.TooSmall;
                    } else if((0.2 <= volumeRatio) && (volumeRatio < 0.95)) {
                        Status = OpeningTaskOutcomingStatus.Correct;
                    } else if((0 < volumeRatio) && (volumeRatio < 0.2)) {
                        Status = OpeningTaskOutcomingStatus.TooBig;
                    } else {
                        Status = OpeningTaskOutcomingStatus.Empty;
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
            using(var filter = GetFilterTasks(openingTasks)) {
                var intersects = filter
                    .Excluding(new ElementId[] { new ElementId(Id) })
                    .WherePasses(GetBoundingBoxFilter())
                    .Cast<FamilyInstance>()
                    .Select(f => new OpeningMepTaskOutcoming(f));
                return openingTasks.Intersect(intersects, _equalityComparer).ToList();
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

        private FilteredElementCollector GetFilterTasks(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            if(IsRemoved) {
                return default;
            }
            return new FilteredElementCollector(GetDocument(), GetTasksIds(openingTasks));
        }

        private ICollection<ElementId> GetTasksIds(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            return openingTasks.Select(t => new ElementId(t.Id)).ToList();
        }

        private string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
            if(_familyInstance is null) {
                throw new ArgumentNullException(nameof(_familyInstance));
            }
            string value = string.Empty;
            if(_familyInstance.IsExistsParam(paramName)) {
                object paramValue = _familyInstance.GetParamValue(paramName);
                if(!(paramValue is null)) {
                    value = paramValue.ToString();
                }
            }
            return value;
        }

    }
}
