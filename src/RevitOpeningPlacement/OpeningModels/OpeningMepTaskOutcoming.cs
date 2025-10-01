using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс, обозначающий экземпляры семейств заданий на отверстия, 
/// размещаемые в файлах-источниках заданий на отверстия для последующей передачи этих заданий получателю
/// </summary>
internal class OpeningMepTaskOutcoming : ISolidProvider, IEquatable<OpeningMepTaskOutcoming>, IFamilyInstanceProvider {
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


    /// <summary>
    /// Создает экземпляр класса <see cref="OpeningMepTaskOutcoming"/>
    /// <para>Примечание: конструктор не обновляет свойство <see cref="Status"/> и <see cref="Host"/>.
    /// Для обновления этого свойства нужно вызвать <see cref="RevitOpeningPlacement.Services.IOpeningInfoUpdater.UpdateInfo"/></para>
    /// </summary>
    /// <param name="openingTaskOutcoming">Экземпляр семейства задания на отверстие, расположенного в текущем документе Revit</param>
    public OpeningMepTaskOutcoming(FamilyInstance openingTaskOutcoming) {
        _familyInstance = openingTaskOutcoming;
        Id = _familyInstance.Id;
        Location = (_familyInstance.Location as LocationPoint).Point;
        OpeningType = RevitRepository.GetOpeningType(openingTaskOutcoming.Symbol.Family.Name);

        Date = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDate);
        MepSystem = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningMepSystem);
        Description = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDescription);
        CenterOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetCenter);
        BottomOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetBottom);
        Comment = _familyInstance.GetParamValueStringOrDefault(
            SystemParamsConfig.Instance.CreateRevitParam(
                _familyInstance.Document,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
            string.Empty);
        Username = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningAuthor);
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
    public ElementId Id { get; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string Comment { get; } = string.Empty;

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    public string Username { get; } = string.Empty;

    /// <summary>
    /// Точка расположения экземпляра семейства задания на отверстие
    /// </summary>
    public XYZ Location { get; }

    /// <summary>
    /// Элемент из связи, который можно считать хостом текущего задания на отверстие.
    /// <para>"Можно считать"  - потому что текущее задание на отверстие может пересекаться с несколькими конструкциями из связи, из которых определяется один элемент</para>
    /// <para>Для обновления использовать <see cref="RevitOpeningPlacement.Services.IOpeningInfoUpdater.UpdateInfo"/></para>
    /// </summary>
    public Element Host { get; set; }

    /// <summary>
    /// Флаг, обозначающий, удален ли экземпляр семейства задания на отверстие из проекта
    /// </summary>
    public bool IsRemoved => (_familyInstance is null) || (!_familyInstance.IsValidObject);

    /// <summary>
    /// Флаг, обозначающий статус исходящего задания на отверстие
    /// <para>Для обновления использовать <see cref="RevitOpeningPlacement.Services.IOpeningInfoUpdater.UpdateInfo"/></para>
    /// </summary>
    public OpeningTaskOutcomingStatus Status { get; set; } = OpeningTaskOutcomingStatus.NotActual;

    /// <summary>
    /// Тип проема
    /// </summary>
    public OpeningType OpeningType { get; } = OpeningType.WallRectangle;


    /// <summary>
    /// Возвращает экземпляр семейства задания на отверстие
    /// </summary>
    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }

    /// <summary>
    /// Возвращает Solid экземпляра семейства задания на отверстие с трансформированными координатами, 
    /// если элемент был удален из документа, будет возвращено значение по умолчанию
    /// </summary>
    public Solid GetSolid() {
        return IsRemoved ? default : _familyInstance.GetSolid();
    }

    /// <summary>
    /// Возвращает BoundingBoxXYZ с учетом расположения <see cref="_familyInstance">элемента</see> в файле Revit, 
    /// если элемент был удален из документа, будет возвращено значение по умолчанию
    /// </summary>
    public BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return IsRemoved ? default : _familyInstance.GetBoundingBox();
    }


    /// <summary>
    /// Проверяет, размещено ли уже такое же задание на отверстие в проекте. 
    /// Под "таким" же понимается 
    /// либо экземпляр семейства задания на отверстие, точка вставки которого и объем 
    ///     не отклоняются от соответствующих величин у текущего задания на отверстие в соответствии с допусками,
    /// либо экземпляр семейства задания на отверстие, которое полностью содержит в себе текущее задание.
    /// </summary>
    /// <param name="placedOpenings">Существующие задания на отверстия в проекте</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
#pragma warning disable 0618
    public bool IsAlreadyPlaced(ICollection<OpeningMepTaskOutcoming> placedOpenings) {
        if(IsRemoved || placedOpenings.Count == 0) {
            return false;
        }
        var closestPlacedOpenings = GetIntersectingTasksRaw(placedOpenings);
        if(ThisOpeningIsCompletelyInsideOther(closestPlacedOpenings)) {
            return true;
        }

        var similarOpenings = closestPlacedOpenings.Where(placedOpening =>
            placedOpening.Location
            .DistanceTo(placedOpening.Location) <= _distance3dTolerance);
        foreach(var placedOpening in similarOpenings) {
            if(placedOpening.EqualsSolid(GetSolid(), XYZExtension.FeetRound)) {
                return true;
            }
        }
        return false;
    }
#pragma warning restore 0618

    public override bool Equals(object obj) {
        return !IsRemoved && (obj is OpeningMepTaskOutcoming otherTask) && Equals(otherTask);
    }

    public override int GetHashCode() {
        return Id.GetIdValue();
    }

    public bool Equals(OpeningMepTaskOutcoming other) {
        return !IsRemoved && (other != null) && (Id == other.Id);
    }

    /// <summary>
    /// Возвращает ограничивающий бокс в координатах активного документа, увеличенный на 0.01 единицу длины Revit по сравнению с боксом по умолчанию.
    /// <para>
    /// Использовать для создания фильтра <see cref="Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/>, в который должны попадать элементы, которые касаются текущего задания на отверстие.
    /// </para>
    /// </summary>
    public BoundingBoxXYZ GetExtendedBoxXYZ() {
        if(IsRemoved) {
            return default;
        }
        var bbox = _familyInstance.GetBoundingBox();
        var minToMaxVector = (bbox.Max - bbox.Min).Normalize();
        double coefficient = 1.01;
        var addition = minToMaxVector.Multiply(coefficient);
        var maxExtended = bbox.Max + addition;
        var minExtended = bbox.Min - addition;
        return new BoundingBoxXYZ() {
            Min = minExtended,
            Max = maxExtended
        };
    }

    /// <summary>
    /// Проверяет, пересекается ли текущее задание на отверстие с другим.
    /// Использовать для определения заданий на отверстия, которые надо объединить.
    /// </summary>
    /// <param name="otherOpening">Другое задание на отверстие из активного файла для проверки</param>
    public bool Intersect(OpeningMepTaskOutcoming otherOpening) {
        if(IsRemoved || otherOpening.IsRemoved) {
            return false;
        }
        var thisSolid = GetSolid();
        if((thisSolid is null) || (thisSolid.Volume <= _volumeTolerance)) {
            return false;
        }
        var otherSolid = otherOpening.GetSolid();
        if((otherSolid is null) || (otherSolid.Volume <= _volumeTolerance)) {
            return false;
        }
        try {
            var intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                thisSolid, otherSolid, BooleanOperationsType.Union);
            if(intersectSolid != null && SolidUtils.SplitVolumes(intersectSolid).Count == 1) {
                return true;
            }
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {

            var thisPlanarFaces = GetPlanarFaces(thisSolid);
            var othersPlanarFaces = GetPlanarFaces(otherSolid);

            foreach(var thisFace in thisPlanarFaces) {
                foreach(var otherFace in othersPlanarFaces) {
                    if(FacesOverlap(thisFace, otherFace)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool FacesOverlap(PlanarFace first, PlanarFace second) {
        // FaceNormal.Negate() для заданий на отверстия, которые касаются снаружи
        // FaceNormal для заданий на отверстия, которые находятся внутри другого и касаются изнутри
        if(first.FaceNormal.IsAlmostEqualTo(second.FaceNormal.Negate())
            || first.FaceNormal.IsAlmostEqualTo(second.FaceNormal)) {
            var firstCurves = first.GetEdgesAsCurveLoops().SelectMany(l => l.ToArray()).ToArray();
            var secondCurves = second.GetEdgesAsCurveLoops().SelectMany(l => l.ToArray()).ToArray();
            foreach(var firstCurve in firstCurves) {
                foreach(var secondCurve in secondCurves) {
                    if(firstCurve.Intersect(secondCurve) != SetComparisonResult.Disjoint) {
                        return true;
                    }
                }
            }
            //var projection = first.Project(second.Origin);
            //if(projection is null || Math.Abs(projection.Distance) < 0.00005 && first.IsInside(projection.UVPoint)) {
            //    // вторая поверхность полностью внутри первой или наоборот
            //    return true;
            //}
        }
        return false;
    }

    /// <summary>
    /// Возвращает коллекцию плоских поверхностей солида
    /// </summary>
    /// <param name="solid">Солид</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private ICollection<PlanarFace> GetPlanarFaces(Solid solid) {
        if(solid is null) { throw new ArgumentNullException(nameof(solid)); }
        HashSet<PlanarFace> result = [];
        var faces = solid.Faces;
        // заполнение в обратном порядке, потому что в конце Faces находятся бОльшие поверхности, которые интересны в первую очередь
        for(int i = faces.Size - 1; i >= 0; i--) {
            var item = faces.get_Item(i);
            if(item is not null and PlanarFace planarFace) {
                result.Add(planarFace);
            }
        }
        return result;
    }

    /// <summary>
    /// Первоначальная быстрая проверка поданной коллекции заданий на отверстия из текущего файла на пересечение с текущим заданием на отверстие
    /// </summary>
    private ICollection<OpeningMepTaskOutcoming> GetIntersectingTasksRaw(ICollection<OpeningMepTaskOutcoming> openingTasks) {
        if(!IsRemoved) {
            var intersects = new FilteredElementCollector(GetDocument(), GetTasksIds(openingTasks))
            .Excluding(new ElementId[] { Id })
            .WherePasses(GetBoundingBoxFilter())
            .WherePasses(new ElementIntersectsSolidFilter(GetSolid()))
            .Cast<FamilyInstance>()
            .Select(famInst => new OpeningMepTaskOutcoming(famInst));
            return openingTasks.Intersect(intersects).ToHashSet();
        } else {
            return Array.Empty<OpeningMepTaskOutcoming>();
        }
    }

    /// <summary>
    /// Проверяет, находится ли экземпляр семейства текущего задания на отверстие внутри какого-либо из коллекции
    /// </summary>
    /// <param name="othersOpeningTasks">Коллекция с заданиями на отверстия для проверки</param>
    /// <returns>True, если какой-либо экземпляр семейства задания на отверстие из поданной коллекции полностью содержит в себе текущее задание, иначе False</returns>
    private bool ThisOpeningIsCompletelyInsideOther(ICollection<OpeningMepTaskOutcoming> othersOpeningTasks) {
        var thisOpeningSolid = GetSolid();
        double thisOpeningSolidVolume = thisOpeningSolid.Volume;
        double intersectionVolumeTolerance = thisOpeningSolidVolume - _volumeTolerance;
        foreach(var openingTask in othersOpeningTasks) {
            var otherSolid = openingTask.GetSolid();
            try {
                double intersectionVolume = BooleanOperationsUtils.ExecuteBooleanOperation(thisOpeningSolid, otherSolid, BooleanOperationsType.Intersect).Volume;
                if(intersectionVolume > intersectionVolumeTolerance) {
                    return true;
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                continue;
            }
        }
        return false;
    }

    private Outline GetOutline() {
        if(IsRemoved) {
            return default;
        }
        var bb = GetTransformedBBoxXYZ();
        return new Outline(bb.Min, bb.Max);
    }

    private BoundingBoxIntersectsFilter GetBoundingBoxFilter() {
        return IsRemoved ? default : new BoundingBoxIntersectsFilter(GetOutline());
    }

    /// <summary>
    /// Возвращает документ, в котором размещено семейство отверстия. 
    /// Если экземпляр семейства удален, будет выброшено исключение <see cref="Autodesk.Revit.Exceptions.InvalidObjectException"/>
    /// Проверку на удаление делать через <see cref="IsRemoved"/>
    /// </summary>
    private Document GetDocument() {
        return IsRemoved ? default : _familyInstance.Document;
    }

    private ICollection<ElementId> GetTasksIds(ICollection<OpeningMepTaskOutcoming> openingTasks) {
        return openingTasks.Where(task => !task.IsRemoved).Select(task => task.Id).ToHashSet();
    }

    /// <summary>
    /// Возвращает строковое значение параметра по названию или пустую строку, если параметр отсутствует у текущего экземпляра семейства задания на отверстие
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
        if(_familyInstance is null) {
            throw new ArgumentNullException(nameof(_familyInstance));
        }
        string value = string.Empty;
        if(_familyInstance.GetParameters(paramName).FirstOrDefault(item => item.IsShared) != null) {
            object paramValue = _familyInstance.GetParamValue(paramName);
            if(paramValue is not null) {
                value = paramValue.ToString();
            }
        }
        return value;
    }
}
