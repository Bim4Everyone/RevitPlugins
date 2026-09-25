using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис обновления информации по входящим заданиям на отверстия из связей.
/// <para>
/// Статус и основа задания определяются относительно элементов конструкций
/// и чистовых отверстий из активного файла - получателя заданий.
/// </para>
/// <para>
/// Свои чистовые отверстия выбираются по разделу активного файла: КР принимает задания
/// и от АР, и от ВИС, АР - только от ВИС.
/// </para>
/// </summary>
internal class OpeningTaskIncomingInfoUpdater : OpeningInfoUpdaterBase<IOpeningTaskIncoming> {
    private readonly RevitRepository _revitRepository;
    private readonly ISolidProviderUtils _solidUtils;
    private readonly IIntersectingElementsFinder _intersectingElementsFinder;
    private readonly IHostFinder _hostFinder;
    private readonly IncomingTaskStatusesSettings _statusesSettings;

    /// <summary>
    /// Элементы конструкций из активного документа
    /// </summary>
    private readonly ICollection<ElementId> _constructureElementsIds;

    /// <summary>
    /// Чистовые отверстия, размещенные в активном документе - получателе заданий
    /// </summary>
    private readonly ICollection<IOpeningReal> _realOpenings;

    public OpeningTaskIncomingInfoUpdater(
        RevitRepository revitRepository,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder,
        IHostFinder hostFinder,
        IDocTypesHandler docTypesHandler,
        IncomingTaskStatusesSettings statusesSettings) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _solidUtils = solidUtils ?? throw new ArgumentNullException(nameof(solidUtils));
        _intersectingElementsFinder = intersectingElementsFinder
            ?? throw new ArgumentNullException(nameof(intersectingElementsFinder));
        _hostFinder = hostFinder ?? throw new ArgumentNullException(nameof(hostFinder));
        _statusesSettings = statusesSettings ?? throw new ArgumentNullException(nameof(statusesSettings));
        if(docTypesHandler is null) {
            throw new ArgumentNullException(nameof(docTypesHandler));
        }

        _realOpenings = docTypesHandler.GetDocType(_revitRepository.Doc) == DocTypeEnum.KR
            ? _revitRepository.GetRealOpeningsKr().ToArray<IOpeningReal>()
            : _revitRepository.GetRealOpeningsAr().ToArray<IOpeningReal>();
        _constructureElementsIds = _revitRepository.GetConstructureElementsIds();
    }


    private protected override void UpdateInfoCore(IOpeningTaskIncoming incomingTask) {
        var taskSolid = incomingTask.GetSolid();
        ICollection<ElementId> intersectingStructureElements = Array.Empty<ElementId>();
        ICollection<ElementId> intersectingOpenings = Array.Empty<ElementId>();
        if(_statusesSettings.CheckIntersections) {
            var taskBBox = incomingTask.GetTransformedBBoxXYZ();
            intersectingStructureElements = _intersectingElementsFinder
                .GetIntersectingElementIds(_revitRepository.Doc, _constructureElementsIds, taskSolid);
            intersectingOpenings = GetIntersectingOpeningsIds(taskSolid, taskBBox);
        }

        if(_statusesSettings.CheckIntersections
           && _statusesSettings.CheckHost) {
            incomingTask.Host = FindHost(taskSolid, intersectingStructureElements, intersectingOpenings);
        }

        if(_statusesSettings.CheckUnacceptableConstructions
            && _intersectingElementsFinder.GetIntersectingElementIds(
                _revitRepository.Doc,
                RevitRepository.UnacceptableStructureCategories.ToArray(),
                taskSolid).Count > 0) {
            incomingTask.Status = OpeningTaskIncomingStatus.UnacceptableConstructions;
            return;
        }

        if(_statusesSettings.CheckIntersections
           && _statusesSettings.CheckDifferentConstructions
           && _hostFinder.InDifferentCategories(
               GetHostElements(intersectingStructureElements, intersectingOpenings))) {
            incomingTask.Status = OpeningTaskIncomingStatus.DifferentConstructions;
            return;
        }

        incomingTask.Status = _statusesSettings.CheckIntersections
            ? GetIntersectionStatus(intersectingStructureElements.Count > 0, intersectingOpenings.Count > 0)
            : OpeningTaskIncomingStatus.New;
    }

    /// <summary>
    /// Возвращает статус сопоставления задания с конструкциями и чистовыми отверстиями активного файла.
    /// </summary>
    /// <param name="intersectsStructures">Задание пересекает конструкции активного файла</param>
    /// <param name="intersectsOpenings">Задание пересекает чистовые отверстия активного файла</param>
    private OpeningTaskIncomingStatus GetIntersectionStatus(bool intersectsStructures, bool intersectsOpenings) {
        if(!intersectsStructures && !intersectsOpenings) {
            return OpeningTaskIncomingStatus.NoIntersection;
        }
        if(intersectsStructures && intersectsOpenings) {
            return OpeningTaskIncomingStatus.NotMatch;
        }
        return intersectsOpenings
            ? OpeningTaskIncomingStatus.Completed
            : OpeningTaskIncomingStatus.New;
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
        var opening = _realOpenings.FirstOrDefault(realOpening =>
            _solidUtils.IntersectsSolid(realOpening, taskSolid, taskBBox));
        return opening != null ? new ElementId[] { opening.Id } : Array.Empty<ElementId>();
    }
}
