using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Считает, насколько чистовое отверстие соответствует элементам из связей.
/// </summary>
internal class RealOpeningVolumeAnalyzer {
    private readonly RevitRepository _revitRepository;
    private readonly ISolidProviderUtils _solidUtils;
    private readonly IIntersectingElementsFinder _intersectingElementsFinder;

    /// <summary>
    /// Кэш Id вентблоков по связям АР
    /// </summary>
    private readonly Dictionary<IConstructureLinkElementsProvider, ICollection<ElementId>> _ventBlockIdsByLink = [];

    public RealOpeningVolumeAnalyzer(
        RevitRepository revitRepository,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _solidUtils = solidUtils ?? throw new ArgumentNullException(nameof(solidUtils));
        _intersectingElementsFinder = intersectingElementsFinder
                                      ?? throw new ArgumentNullException(nameof(intersectingElementsFinder));
    }

    /// <summary>
    /// Анализирует чистовое отверстие относительно элементов ВИС и заданий на отверстия из связей ВИС
    /// </summary>
    /// <param name="opening">Чистовое отверстие из активного файла</param>
    /// <param name="mepLinks">Связи с элементами ВИС и заданиями на отверстия</param>
    /// <param name="settings">Настройки проверки статусов чистовых отверстий</param>
    /// <returns>Если отверстие не актуально (NotActual == true),
    /// отношение объемов рассчитано не будет (VolumeRatio == 0)</returns>
    public (bool NotActual, double VolumeRatio) AnalyzeMepLinks(
        IOpeningReal opening,
        ICollection<IMepLinkElementsProvider> mepLinks,
        IRealOpeningStatusesSettings settings) {
        if(!settings.CheckNotActual
           && !settings.CheckVolumeMatch) {
            return (false, 0);
        }

        var openingSolid = opening.GetSolid();
        var solidAfterIntersection = openingSolid;

        foreach(var link in mepLinks) {
            var intersectingLinkElements = GetIntersectingLinkElements(opening, link);
            if(intersectingLinkElements.Count == 0) {
                continue;
            }

            if(settings.CheckNotActual
               && LinkElementsIntersectHost(opening, link, intersectingLinkElements)) {
                return (true, 0);
            }

            if(settings.CheckVolumeMatch) {
                solidAfterIntersection = _solidUtils.SubtractSolids(
                    solidAfterIntersection,
                    GetLinkElementsSolids(link, intersectingLinkElements));
            }
        }

        return (false, settings.CheckVolumeMatch ? GetVolumeRatio(openingSolid, solidAfterIntersection) : 0);
    }

    /// <summary>
    /// Анализирует чистовое отверстие КР относительно заданий на отверстия из связей АР
    /// </summary>
    public (bool NotActual, double VolumeRatio) AnalyzeArLinks(
        IOpeningReal opening,
        ICollection<IConstructureLinkElementsProvider> arLinks,
        IRealOpeningStatusesSettings settings) {
        bool checkNotActual = settings.CheckNotActual;
        bool checkVolumeMatch = settings.CheckVolumeMatch;
        if(!checkNotActual
           && !checkVolumeMatch) {
            return (false, 0);
        }

        var openingSolid = opening.GetSolid();
        var solidAfterIntersection = openingSolid;

        foreach(var link in arLinks) {
            solidAfterIntersection = SubtractLinkOpenings(
                opening,
                link,
                solidAfterIntersection,
                checkNotActual,
                checkVolumeMatch,
                out bool openingIsNotActual);
            if(checkNotActual && openingIsNotActual) {
                return (true, 0);
            }
        }

        return (false, checkVolumeMatch ? GetVolumeRatio(openingSolid, solidAfterIntersection) : 0);
    }

    /// <summary>
    /// Вычитает из солида чистового отверстия КР солиды заданий на отверстия из связи АР
    /// </summary>
    /// <param name="opening">Чистовое отверстие КР из активного файла</param>
    /// <param name="link">Связь АР</param>
    /// <param name="solidForSubtraction">
    /// Солид чистового отверстия в координатах активного файла, из которого уже вычтены задания предыдущих связей
    /// </param>
    /// <param name="checkNotActual">
    /// Нужно ли проверять, пересекают ли задания из связи основу чистового отверстия
    /// </param>
    /// <param name="checkVolumeMatch">
    /// Нужно ли вычитать солиды заданий: результат вычитания нужен только для расчета доли заполнения
    /// </param>
    /// <param name="linkOpeningsIntersectConstructions">
    /// Флаг, показывающий, полностью ли чистовое отверстие закрывает собой пересекающие его задания
    /// </param>
    private Solid SubtractLinkOpenings(
        IOpeningReal opening,
        IConstructureLinkElementsProvider link,
        Solid solidForSubtraction,
        bool checkNotActual,
        bool checkVolumeMatch,
        out bool linkOpeningsIntersectConstructions) {
        var openingSolid = opening.GetSolid();
        var openingSolidInLinkCoordinates = link.ToLinkCoordinates(openingSolid);
        var openingBBoxInLinkCoordinates = link.ToLinkCoordinates(opening.GetTransformedBBoxXYZ());

        ICollection<Solid> intersectingTasksSolids = link
            .GetOpeningsReal()
            .Where(openingTask => _solidUtils.IntersectsSolid(
                openingTask,
                openingSolidInLinkCoordinates,
                openingBBoxInLinkCoordinates))
            .Select(task => link.ToActiveDocCoordinates(task.GetSolid()))
            .Concat(GetIntersectingVentBlockSolids(opening, link, openingSolidInLinkCoordinates))
            .ToHashSet();

        if(checkNotActual) {
            var hostSolid = opening.GetHost().GetSolid();
            linkOpeningsIntersectConstructions = intersectingTasksSolids
                .Any(taskSolid => TaskIntersectsHost(taskSolid, hostSolid));
        } else {
            linkOpeningsIntersectConstructions = false;
        }

        return checkVolumeMatch
            ? _solidUtils.SubtractSolids(solidForSubtraction, intersectingTasksSolids)
            : solidForSubtraction;
    }

    /// <summary>
    /// Возвращает долю объема чистового отверстия, которую пересекают элементы из связей
    /// </summary>
    /// <param name="openingSolid">Исходный солид чистового отверстия</param>
    /// <param name="solidAfterIntersection">
    /// Солид чистового отверстия после вычитания солидов элементов из связей, которые его пересекают
    /// </param>
    /// <returns>
    /// 0 - солид чистового отверстия не пересекается ни с одним элементом из связей,
    /// 1 - элементы из связей пересекают солид чистового отверстия целиком
    /// </returns>
    public double GetVolumeRatio(Solid openingSolid, Solid solidAfterIntersection) {
        return openingSolid.Volume == 0 ? 1 : 1 - solidAfterIntersection.Volume / openingSolid.Volume;
    }

    /// <summary>
    /// Возвращает коллекцию Id элементов ВИС и заданий на отверстия из связи,
    /// которые пересекаются с чистовым отверстием
    /// </summary>
    private ICollection<ElementId> GetIntersectingLinkElements(
        IOpeningReal opening,
        IMepLinkElementsProvider link) {
        var openingSolidInLinkCoordinates = link.ToLinkCoordinates(opening.GetSolid());

        var elements = _intersectingElementsFinder
            .GetIntersectingElementIds(link.Document, link.GetMepElementIds(), openingSolidInLinkCoordinates)
            .ToHashSet();
        elements.UnionWith(
            _intersectingElementsFinder
                .GetIntersectingElementIds(link.Document, link.GetOpeningsTaskIds(), openingSolidInLinkCoordinates));

        return elements;
    }

    /// <summary>
    /// Проверяет, пересекаются ли заданные элементы из связи с хостом чистового отверстия
    /// </summary>
    private bool LinkElementsIntersectHost(
        IOpeningReal opening,
        IMepLinkElementsProvider link,
        ICollection<ElementId> linkElementsForChecking) {
        var hostSolidInLinkCoordinates = link.ToLinkCoordinates(opening.GetHost().GetSolid());
        return _intersectingElementsFinder
            .GetIntersectingElementIds(link.Document, linkElementsForChecking, hostSolidInLinkCoordinates)
            .Any();
    }

    /// <summary>
    /// Возвращает коллекцию солидов заданных элементов из связи
    /// в координатах активного файла с чистовым отверстием
    /// </summary>
    private ICollection<Solid> GetLinkElementsSolids(
        IMepLinkElementsProvider link,
        ICollection<ElementId> linkElementsIds) {
        List<Solid> solids = [];
        foreach(var id in linkElementsIds) {
            var solid = link.Document.GetElement(id)?.GetSolid();
            if(solid is null) {
                continue;
            }

            try {
                solids.Add(link.ToActiveDocCoordinates(solid));
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                continue;
            }
        }

        return solids;
    }

    /// <summary>
    /// Возвращает солиды вентблоков из связи АР, которые пересекают чистовое отверстие КР,
    /// в координатах активного файла.
    /// <para>
    /// Вентблоки не являются чистовыми отверстиями, поэтому в <see cref="IConstructureLinkElementsProvider"/>
    /// их нет и они ищутся напрямую в документе связи.
    /// </para>
    /// </summary>
    /// <param name="link">Связь АР</param>
    /// <param name="openingSolidInLinkCoordinates">
    /// Солид чистового отверстия КР в координатах связи</param>
    private IEnumerable<Solid> GetIntersectingVentBlockSolids(
        IOpeningReal opening,
        IConstructureLinkElementsProvider link,
        Solid openingSolidInLinkCoordinates) {
        var ventBlockIds = GetVentBlockIds(link);
        if(ventBlockIds.Count == 0) {
            yield break;
        }

        // грубый отбор по боксу. ElementIntersectsSolidFilter здесь неприменим:
        // он проверяет собственную геометрию экземпляра, а у вентблока ее нет -
        // тело лежит во вложенном общем семействе, то есть в отдельном элементе связи
        var candidateIds = new FilteredElementCollector(link.Document, ventBlockIds)
            .WherePasses(new BoundingBoxIntersectsFilter(openingSolidInLinkCoordinates.GetOutline()))
            .ToElementIds();

        // точная проверка пересечением солидов.
        // VentBlockAr.GetSolid уже применяет трансформацию связи,
        // поэтому сравнение идет в координатах активного документа
        var openingSolid = opening.GetSolid();
        var openingBBox = opening.GetTransformedBBoxXYZ();
        foreach(var id in candidateIds) {
            if(link.Document.GetElement(id) is not FamilyInstance instance) {
                continue;
            }

            var ventBlock = new VentBlockAr(instance, link.DocumentTransform);
            if(_solidUtils.IntersectsSolid(ventBlock, openingSolid, openingBBox)) {
                yield return ventBlock.GetSolid();
            }
        }
    }

    /// <summary>
    /// Возвращает Id вентблоков из документа связи АР с кэшированием по связи
    /// </summary>
    private ICollection<ElementId> GetVentBlockIds(IConstructureLinkElementsProvider link) {
        if(!_ventBlockIdsByLink.TryGetValue(link, out var ids)) {
            ids = _revitRepository.GetFamilyInstances(
                    link.Document,
                    RevitRepository.VentBlockArFamilyName,
                    RevitRepository.VentBlockCategory)
                .Select(ventBlock => ventBlock.Id)
                .ToArray();
            _ventBlockIdsByLink.Add(link, ids);
        }

        return ids;
    }

    /// <summary>
    /// Проверяет, пересекает ли задание из связи основу чистового отверстия КР.
    /// </summary>
    /// <param name="taskSolid">Солид задания на отверстие в координатах активного файла</param>
    /// <param name="hostSolid">Солид основы чистового отверстия, то есть конструкции с вырезом</param>
    private bool TaskIntersectsHost(Solid taskSolid, Solid hostSolid) {
        try {
            return BooleanOperationsUtils.ExecuteBooleanOperation(
                           taskSolid,
                           hostSolid,
                           BooleanOperationsType.Intersect)
                       ?.Volume
                   > ConstantsProvider.ToleranceVolumeFeetCube;
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            return false;
        }
    }
}
