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
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс для обертки проема из связанного файла АР, подгруженного в активный документ КР
/// </summary>
internal class OpeningArTaskIncoming : OpeningRealBase, IEquatable<OpeningArTaskIncoming>, IOpeningTaskIncoming {
    private readonly RevitRepository _revitRepository;

    /// <summary>
    /// Конструктор класса для обертки проема из связанного файла АР, подгруженного в активный документ КР
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа КР</param>
    /// <param name="openingTask">Задание на отверстие от АР</param>
    /// <param name="transform">
    /// Трансформация файла задания на отверстие от АР относительно активного документа КР</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OpeningArTaskIncoming(RevitRepository revitRepository, FamilyInstance openingTask, Transform transform)
        : base(openingTask) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        FileName = _familyInstance.Document.PathName;
        Transform = transform;
        Location = Transform.OfPoint((_familyInstance.Location as LocationPoint).Point);
        // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
        Rotation = (_familyInstance.Location as LocationPoint).Rotation
            + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);
        OpeningType = RevitRepository.GetOpeningType(_familyInstance.Symbol.FamilyName);

        DisplayDiameter = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArDiameter);
        DisplayWidth = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArWidth);
        DisplayHeight = GetFamilyInstanceStringParamValueOrEmpty(RealOpeningArPlacer.RealOpeningArHeight);
        Comment = _familyInstance.GetParamValueStringOrDefault(
            SystemParamsConfig.Instance.CreateRevitParam(
                _familyInstance.Document,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
            string.Empty);

        Diameter = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArDiameter);
        Height = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArHeight);
        Width = GetFamilyInstanceDoubleParamValueOrZero(RealOpeningArPlacer.RealOpeningArWidth);
    }


    public string FileName { get; }


    /// <summary>
    /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
    /// </summary>
    public Transform Transform { get; } = Transform.Identity;

    /// <summary>
    /// Точка расположения экземпляра семейства входящего задания на отверстие 
    /// в координатах активного документа - получателя заданий
    /// </summary>
    public XYZ Location { get; }

    /// <summary>
    /// Угол поворота задания на отверстие в радианах в координатах активного файла, 
    /// в который подгружена связь с заданием на отверстие от АР
    /// </summary>
    public double Rotation { get; }

    public string Comment { get; } = string.Empty;

    public OpeningType OpeningType { get; } = OpeningType.WallRectangle;

    /// <summary>
    /// Статус входящего задания на отверстие от АР
    /// <para>Для обновления использовать метод <see cref="UpdateStatusAndHost"/></para>
    /// </summary>
    public OpeningTaskIncomingStatus Status { get; private set; } = OpeningTaskIncomingStatus.New;

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
    /// Хост входящего задания на отверстие из активного документа
    /// </summary>
    public Element Host { get; private set; }


    public override bool Equals(object obj) {
        return (obj is OpeningArTaskIncoming opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return (int) (Id.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(OpeningArTaskIncoming other) {
        return (other != null)
            && (Id == other.Id)
            && FileName.Equals(other.FileName, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Возвращает солид архитектурного проема в координатах активного файла (КР) - получателя заданий на отверстия
    /// </summary>
    public override Solid GetSolid() {
        return SolidUtils.CreateTransformed(GetOpeningSolid(), Transform);
    }

    /// <summary>
    /// Возвращает BBox в координатах активного документа-получателя (КР) заданий на отверстия
    /// </summary>
    public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return GetSolid().GetTransformedBoundingBox();
    }

    /// <summary>
    /// Обновляет <see cref="Status"/> входящего задания на отверстие
    /// </summary>
    /// <param name="realOpenings">Коллекция чистовых отверстий КР, 
    /// размещенных в активном КР документе-получателе заданий на отверстия</param>
    /// <param name="constructureElementsIds">
    /// Коллекция элементов конструкций из активного документа-получателя заданий</param>
    public void UpdateStatusAndHost(
        ICollection<OpeningRealKr> realOpenings,
        ICollection<ElementId> constructureElementsIds) {
        try {
            var thisOpeningSolid = GetSolid();
            var thisOpeningBBox = GetTransformedBBoxXYZ();

            var intersectingStructureElements = GetIntersectingStructureElementsIds(
                thisOpeningSolid,
                constructureElementsIds);
            var intersectingOpenings = GetIntersectingOpeningsIds(realOpenings, thisOpeningSolid, thisOpeningBBox);

            Host = FindHost(thisOpeningSolid, intersectingStructureElements, intersectingOpenings);
            if(GetIntersectingStructureElementsIds(thisOpeningSolid,
                RevitRepository.UnacceptableStructureCategories.ToArray())
                .Count() > 0) {
                Status = OpeningTaskIncomingStatus.UnacceptableConstructions;
                return;
            }
            if(GetHostCategories(intersectingStructureElements, intersectingOpenings).Count > 1) {
                Status = OpeningTaskIncomingStatus.DifferentConstructions;
                return;
            }

            if((intersectingStructureElements.Count == 0) && (intersectingOpenings.Count == 0)) {
                Status = OpeningTaskIncomingStatus.NoIntersection;
            } else if((intersectingStructureElements.Count > 0) && (intersectingOpenings.Count == 0)) {
                Status = OpeningTaskIncomingStatus.New;
            } else if((intersectingStructureElements.Count > 0) && (intersectingOpenings.Count > 0)) {
                Status = OpeningTaskIncomingStatus.NotMatch;
            } else if((intersectingStructureElements.Count == 0) && (intersectingOpenings.Count > 0)) {
                Status = OpeningTaskIncomingStatus.Completed;
            }
        } catch(Exception ex) when(
            ex is Autodesk.Revit.Exceptions.ApplicationException
            or NullReferenceException
            or ArgumentNullException) {
            Status = OpeningTaskIncomingStatus.Invalid;
        }
    }


    /// <summary>
    /// Находит категории конструкций, в которых расположено текущее входящее задание на отверстие из связи
    /// </summary>
    /// <param name="intersectingStructureElements">Элементы конструкций из активного файла, 
    /// которые пересекаются с входящим заданием на отверстие</param>
    /// <param name="intersectingOpenings">Чистовые отверстия из активного файла,
    /// которые пересекаются с входящим заданием на отверстие</param>
    private ICollection<BuiltInCategory> GetHostCategories(
        ICollection<ElementId> intersectingStructureElements,
        ICollection<ElementId> intersectingOpenings) {
        return intersectingStructureElements
            .Select(_revitRepository.Doc.GetElement)
            .Union(intersectingOpenings
                .Select(id => (_revitRepository.Doc.GetElement(id) as FamilyInstance)?.Host)
                .Where(e => e != null))
            .Select(el => el.Category.GetBuiltInCategory())
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Возвращает Id элемента конструкции, который наиболее похож на хост для задания на отверстие.
    /// <para>Под наиболее подходящим понимается элемент конструкции, с которым пересечение наибольшего объема, 
    /// либо хост чистового отверстия, с которым пересекается задание на отверстие.</para> 
    /// </summary>
    /// <param name="thisOpeningSolid">
    /// Солид текущего задания на отверстие в координатах активного файла-получателя заданий</param>
    /// <param name="intersectingStructureElementsIds">
    /// Коллекция Id элементов конструкций из активного документа, с которыми пересекается задание на отверстие
    /// </param>
    /// <param name="intersectingOpeningsIds">
    /// Коллекция Id чистовых отверстий из активного документа, с которыми пересекается задание на отверсите</param>
    private Element FindHost(
        Solid thisOpeningSolid,
        ICollection<ElementId> intersectingStructureElementsIds,
        ICollection<ElementId> intersectingOpeningsIds) {

        if((intersectingOpeningsIds != null) && intersectingOpeningsIds.Any()) {
            return (_revitRepository.GetElement(intersectingOpeningsIds.First()) as FamilyInstance)?.Host;

        } else if((thisOpeningSolid != null)
            && (thisOpeningSolid.Volume > 0)
            && (intersectingStructureElementsIds != null)
            && intersectingStructureElementsIds.Any()) {

            // поиск элемента конструкции, с которым пересечение задания на отверстие имеет наибольший объем
            double halfOpeningVolume = thisOpeningSolid.Volume / 2;
            double intersectingVolumePrevious = 0;
            var hostId = intersectingStructureElementsIds.First();
            foreach(var structureElementId in intersectingStructureElementsIds) {
                var structureSolid = _revitRepository.GetElement(structureElementId)?.GetSolid();
                if((structureSolid != null) && (structureSolid.Volume > 0)) {
                    try {
                        double intersectingVolumeCurrent
                            = BooleanOperationsUtils.ExecuteBooleanOperation(
                                thisOpeningSolid,
                                structureSolid,
                                BooleanOperationsType.Intersect)?.Volume ?? 0;
                        if(intersectingVolumeCurrent >= halfOpeningVolume) {
                            return _revitRepository.GetElement(structureElementId);
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
            return _revitRepository.GetElement(hostId);
        } else {
            return default;
        }
    }

    /// <summary>
    /// Возвращает коллекцию элементов конструкций из активного документа, 
    /// с которыми пересекается текущее задание на отверстие из связи
    /// </summary>
    /// <param name="thisOpeningSolid">
    /// Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
    /// <param name="constructureElementsIds">
    /// Коллекция id элементов конструкций из активного документа ревита</param>
    private ICollection<ElementId> GetIntersectingStructureElementsIds(
        Solid thisOpeningSolid,
        ICollection<ElementId> constructureElementsIds) {
        return (thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0) || (!constructureElementsIds.Any())
            ? Array.Empty<ElementId>()
            : new FilteredElementCollector(_revitRepository.Doc, constructureElementsIds)
                .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
                .ToElementIds();
    }

    /// <summary>
    /// Возвращает коллекцию элементов конструкций заданных категорий из активного документа, 
    /// с которыми пересекается текущее задание на отверстие из связи
    /// </summary>
    /// <param name="thisOpeningSolid">
    /// Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
    /// <param name="constructureCategories">
    /// Коллекция категорий конструкций из активного документа ревита</param>
    private ICollection<ElementId> GetIntersectingStructureElementsIds(
        Solid thisOpeningSolid,
        ICollection<BuiltInCategory> constructureCategories) {
        return new FilteredElementCollector(_revitRepository.Doc)
            .WherePasses(new ElementMulticategoryFilter(constructureCategories))
            .WherePasses(new BoundingBoxIntersectsFilter(thisOpeningSolid.GetOutline()))
            .WherePasses(new ElementIntersectsSolidFilter(thisOpeningSolid))
            .ToElementIds();
    }

    /// <summary>
    /// Возвращает коллекцию Id проемов из активного документа, 
    /// которые пересекаются с текущим заданием на отверстие из связи
    /// </summary>
    /// <param name="realOpenings">Коллекция чистовых отверстий из активного документа ревита</param>
    /// <param name="thisOpeningSolid">
    /// Солид текущего задания на отверстие в координатах активного файла - получателя заданий</param>
    /// <param name="thisOpeningBBox">
    /// Бокс текущего задания на отверстие в координатах активного файла - получателя заданий</param>
#pragma warning disable 0618
    private ICollection<ElementId> GetIntersectingOpeningsIds(
        ICollection<OpeningRealKr> realOpenings,
        Solid thisOpeningSolid,
        BoundingBoxXYZ thisOpeningBBox) {
        if((thisOpeningSolid is null) || (thisOpeningSolid.Volume <= 0)) {
            return Array.Empty<ElementId>();
        } else {
            var opening = realOpenings.FirstOrDefault(
                realOpening => realOpening.IntersectsSolid(thisOpeningSolid, thisOpeningBBox));
            return opening != null ? (new ElementId[] { opening.Id }) : Array.Empty<ElementId>();
        }
    }
#pragma warning restore 0618
}
