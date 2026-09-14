using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Базовый класс сервиса обновления информации по входящим заданиям на отверстия из связей.
/// <para>
/// Статус и основа задания определяются относительно элементов конструкций
/// и чистовых отверстий из активного файла - получателя заданий.
/// </para>
/// </summary>
internal abstract class OpeningTaskIncomingInfoUpdaterBase : OpeningInfoUpdaterBase<IOpeningTaskIncoming> {
    private protected readonly RevitRepository _revitRepository;
    private readonly ISolidProviderUtils _solidUtils;
    private readonly IIntersectingElementsFinder _intersectingElementsFinder;
    private readonly IHostFinder _hostFinder;

    /// <summary>
    /// Элементы конструкций из активного документа
    /// </summary>
    private readonly ICollection<ElementId> _constructureElementsIds;

    protected OpeningTaskIncomingInfoUpdaterBase(
        RevitRepository revitRepository,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder,
        IHostFinder hostFinder) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _solidUtils = solidUtils ?? throw new ArgumentNullException(nameof(solidUtils));
        _intersectingElementsFinder = intersectingElementsFinder
            ?? throw new ArgumentNullException(nameof(intersectingElementsFinder));
        _hostFinder = hostFinder ?? throw new ArgumentNullException(nameof(hostFinder));

        _constructureElementsIds = _revitRepository.GetConstructureElementsIds();
    }


    /// <summary>
    /// Чистовые отверстия, размещенные в активном документе - получателе заданий
    /// </summary>
    private protected abstract ICollection<IOpeningReal> RealOpenings { get; }


    private protected override void UpdateInfoCore(IOpeningTaskIncoming incomingTask) {
        var taskSolid = incomingTask.GetSolid();
        var taskBBox = incomingTask.GetTransformedBBoxXYZ();

        var intersectingStructureElements = _intersectingElementsFinder
            .GetIntersectingElementIds(_revitRepository.Doc, _constructureElementsIds, taskSolid);
        var intersectingOpenings = GetIntersectingOpeningsIds(taskSolid, taskBBox);

        incomingTask.Host = FindHost(taskSolid, intersectingStructureElements, intersectingOpenings);

        if(_intersectingElementsFinder.GetIntersectingElementIds(
                _revitRepository.Doc,
                RevitRepository.UnacceptableStructureCategories.ToArray(),
                taskSolid).Count > 0) {
            incomingTask.Status = OpeningTaskIncomingStatus.UnacceptableConstructions;
            return;
        }
        if(_hostFinder.InDifferentCategories(
                GetHostElements(intersectingStructureElements, intersectingOpenings))) {
            incomingTask.Status = OpeningTaskIncomingStatus.DifferentConstructions;
            return;
        }

        bool intersectsStructures = intersectingStructureElements.Count > 0;
        bool intersectsOpenings = intersectingOpenings.Count > 0;
        if(!intersectsStructures && !intersectsOpenings) {
            incomingTask.Status = OpeningTaskIncomingStatus.NoIntersection;
        } else if(intersectsStructures && !intersectsOpenings) {
            incomingTask.Status = OpeningTaskIncomingStatus.New;
        } else if(intersectsStructures && intersectsOpenings) {
            incomingTask.Status = OpeningTaskIncomingStatus.NotMatch;
        } else {
            incomingTask.Status = OpeningTaskIncomingStatus.Completed;
        }
    }

    private protected override void SetInvalidStatus(IOpeningTaskIncoming incomingTask) {
        incomingTask.Status = OpeningTaskIncomingStatus.Invalid;
    }


    /// <summary>
    /// Возвращает элементы конструкций, в которых расположено входящее задание на отверстие
    /// </summary>
    /// <param name="intersectingStructureElements">Элементы конструкций из активного файла,
    /// которые пересекаются с входящим заданием на отверстие</param>
    /// <param name="intersectingOpenings">Чистовые отверстия из активного файла,
    /// которые пересекаются с входящим заданием на отверстие</param>
    private ICollection<Element> GetHostElements(
        ICollection<ElementId> intersectingStructureElements,
        ICollection<ElementId> intersectingOpenings) {
        return intersectingStructureElements
            .Select(_revitRepository.Doc.GetElement)
            .Union(intersectingOpenings
                .Select(id => (_revitRepository.Doc.GetElement(id) as FamilyInstance)?.Host)
                .Where(e => e != null))
            .ToArray();
    }

    /// <summary>
    /// Возвращает элемент конструкции, который наиболее похож на основу для задания на отверстие.
    /// <para>Под наиболее подходящим понимается элемент конструкции, с которым пересечение наибольшего объема,
    /// либо основа чистового отверстия, с которым пересекается задание на отверстие.</para>
    /// </summary>
    private Element FindHost(
        Solid taskSolid,
        ICollection<ElementId> intersectingStructureElementsIds,
        ICollection<ElementId> intersectingOpeningsIds) {

        return intersectingOpeningsIds.Any()
            ? (_revitRepository.GetElement(intersectingOpeningsIds.First()) as FamilyInstance)?.Host
            : _hostFinder.FindBestHost(
                taskSolid,
                intersectingStructureElementsIds.Select(_revitRepository.GetElement).ToArray());
    }

    /// <summary>
    /// Возвращает коллекцию Id чистовых отверстий из активного документа,
    /// которые пересекаются с входящим заданием на отверстие
    /// </summary>
    private ICollection<ElementId> GetIntersectingOpeningsIds(Solid taskSolid, BoundingBoxXYZ taskBBox) {
        if((taskSolid is null) || (taskSolid.Volume <= 0)) {
            return Array.Empty<ElementId>();
        }
        // для ускорения ищется только первое пересечение
        var opening = RealOpenings.FirstOrDefault(realOpening =>
            _solidUtils.IntersectsSolid(realOpening, taskSolid, taskBBox));
        return opening != null ? new ElementId[] { opening.Id } : Array.Empty<ElementId>();
    }
}
