using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис обновления информации по чистовым отверстиям КР из активного файла КР.
/// <para>
/// Статус определяется относительно связей АР или связей ВИС в зависимости от
/// <see cref="OpeningRealsKrConfig.PlacementType"/>.
/// </para>
/// </summary>
internal class OpeningRealKrInfoUpdater : OpeningRealInfoUpdaterBase<OpeningRealKr> {
    private readonly OpeningRealKrPlacementType _placementType;

    /// <summary>
    /// Связи АР с заданиями на отверстия. Заполняются в режиме <see cref="OpeningRealKrPlacementType.PlaceByAr"/>
    /// </summary>
    private readonly ICollection<IConstructureLinkElementsProvider> _arLinks;

    /// <summary>
    /// Связи ВИС. Заполняются в режиме <see cref="OpeningRealKrPlacementType.PlaceByMep"/>
    /// </summary>
    private readonly ICollection<IMepLinkElementsProvider> _mepLinks;

    private readonly RevitRepository _revitRepository;
    private readonly ILengthConverter _lengthConverter;

    /// <summary>
    /// Минимальное допустимое расстояние между чистовыми отверстиями КР в единицах длины Revit (футах).
    /// <para>0 - проверка выключена</para>
    /// </summary>
    private readonly double _minDistance;

    /// <summary>
    /// Id чистовых отверстий КР активного файла, которые еще могут оказаться слишком близко к соседям.
    /// <para>
    /// Отверстие, для которого доказано отсутствие близких соседей, из набора удаляется:
    /// отношение "слишком близко" симметрично, поэтому такое отверстие уже никого не "поймает".
    /// </para>
    /// </summary>
    private readonly HashSet<ElementId> _proximityPool;

    /// <summary>
    /// Чистовые отверстия КР активного файла для проверки расстояния между ними
    /// </summary>
    private readonly ICollection<OpeningRealKr> _realOpenings;

    /// <summary>
    /// Кэш Id вентблоков по связям АР
    /// </summary>
    private readonly Dictionary<IConstructureLinkElementsProvider, ICollection<ElementId>> _ventBlockIdsByLink = [];

    public OpeningRealKrInfoUpdater(
        RevitRepository revitRepository,
        OpeningRealsKrConfig config,
        ILengthConverter lengthConverter,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder)
        : base(solidUtils, intersectingElementsFinder) {
        if(config is null) {
            throw new ArgumentNullException(nameof(config));
        }

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _lengthConverter = lengthConverter ?? throw new ArgumentNullException(nameof(lengthConverter));
        _placementType = config.PlacementType;
        _minDistance = _lengthConverter.ConvertToInternal(config.MinDistanceBetweenOpenings);
        _realOpenings = _minDistance > 0 ? revitRepository.GetRealOpeningsKr() : [];
        _proximityPool = _realOpenings.Select(opening => opening.Id).ToHashSet();
        if(_placementType == OpeningRealKrPlacementType.PlaceByAr) {
            _arLinks = revitRepository.GetSelectedRevitLinks()
                .Select(link => new ConstructureLinkElementsProvider(revitRepository, link)
                    as IConstructureLinkElementsProvider)
                .ToArray();
            _mepLinks = [];
        } else {
            _arLinks = [];
            _mepLinks = revitRepository.GetSelectedRevitLinks()
                .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
                .ToArray();
        }
    }

    private protected override double EmptyVolumeRatio => 0.01;

    private protected override double TooBigVolumeRatio => 0.5;

    private protected override void UpdateInfoCore(OpeningRealKr opening) {
        switch(_placementType) {
            case OpeningRealKrPlacementType.PlaceByAr:
                UpdateStatusByArLinks(opening);
                break;
            case OpeningRealKrPlacementType.PlaceByMep:
                UpdateStatusByMepLinks(opening, _mepLinks);
                break;
            default:
                throw new InvalidOperationException(
                    $"Режим обработки заданий для КР: '{_placementType}' не поддерживается.");
        }

        UpdateStatusByProximity(opening);
    }

    private protected override void SetStatus(OpeningRealKr opening, OpeningRealStatus status) {
        opening.Status = status;
    }

    /// <summary>
    /// Понижает статус чистового отверстия до <see cref="OpeningRealStatus.TooClose"/>,
    /// если рядом с ним есть другое чистовое отверстие КР.
    /// <para>
    /// Проверяются только отверстия со статусом <see cref="OpeningRealStatus.Correct"/>:
    /// остальные статусы обозначают более серьезные проблемы и не должны прятаться за близостью.
    /// </para>
    /// </summary>
    private void UpdateStatusByProximity(OpeningRealKr opening) {
        if((_minDistance <= 0)
           || (opening.Status != OpeningRealStatus.Correct)) {
            return;
        }

        var neighbourIds = _proximityPool.Where(id => id != opening.Id).ToArray();
        if(neighbourIds.Length == 0) {
            _proximityPool.Remove(opening.Id);
            return;
        }

        // грубый отбор кандидатов по раздутому во все стороны боксу
        var candidateIds = new FilteredElementCollector(_revitRepository.Doc, neighbourIds)
            .WherePasses(new BoundingBoxIntersectsFilter(GetInflatedOutline(opening)))
            .ToElementIds();
        if(candidateIds.Count == 0) {
            _proximityPool.Remove(opening.Id);
            return;
        }

        // точная проверка: раздувается только проверяемое отверстие и только в плоскости,
        // перпендикулярной его оси
        var inflatedSolid = opening.GetInflatedSolid(_minDistance);
        var inflatedBBox = inflatedSolid.GetTransformedBoundingBox();
        bool tooClose = _realOpenings
            .Where(neighbour => candidateIds.Contains(neighbour.Id))
            .Any(neighbour => _solidUtils.IntersectsSolid(neighbour, inflatedSolid, inflatedBBox));

        if(tooClose) {
            opening.Status = OpeningRealStatus.TooClose;
        } else {
            _proximityPool.Remove(opening.Id);
        }
    }

    /// <summary>
    /// Возвращает бокс чистового отверстия, раздутый на минимальное допустимое расстояние по всем осям
    /// </summary>
    private Outline GetInflatedOutline(OpeningRealKr opening) {
        var bbox = opening.GetTransformedBBoxXYZ();
        var offset = new XYZ(_minDistance, _minDistance, _minDistance);
        return new Outline(bbox.Min - offset, bbox.Max + offset);
    }

    /// <summary>
    /// Определяет статус чистового отверстия КР относительно заданий на отверстия из связей АР
    /// </summary>
    private void UpdateStatusByArLinks(OpeningRealKr opening) {
        var openingSolid = opening.GetSolid();
        var solidAfterIntersection = openingSolid;

        foreach(var link in _arLinks) {
            solidAfterIntersection = SubtractLinkOpenings(
                opening,
                link,
                solidAfterIntersection,
                out bool openingIsNotActual);
            if(openingIsNotActual) {
                SetStatus(opening, OpeningRealStatus.NotActual);
                return;
            }
        }

        SetStatus(opening, GetStatusByVolumeRatio(GetSolidsVolumesRatio(openingSolid, solidAfterIntersection)));
    }

    /// <summary>
    /// Вычитает из солида чистового отверстия КР солиды заданий на отверстия из связи АР
    /// </summary>
    /// <param name="opening">Чистовое отверстие КР из активного файла</param>
    /// <param name="link">Связь АР</param>
    /// <param name="solidForSubtraction">
    /// Солид чистового отверстия в координатах активного файла, из которого уже вычтены задания предыдущих связей
    /// </param>
    /// <param name="linkOpeningsIntersectConstructions">
    /// Флаг, показывающий, полностью ли чистовое отверстие закрывает собой пересекающие его задания
    /// </param>
    private Solid SubtractLinkOpenings(
        OpeningRealKr opening,
        IConstructureLinkElementsProvider link,
        Solid solidForSubtraction,
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

        var hostSolid = opening.GetHost().GetSolid();
        linkOpeningsIntersectConstructions = intersectingTasksSolids
            .Any(taskSolid => TaskIntersectsHost(taskSolid, hostSolid));

        return _solidUtils.SubtractSolids(solidForSubtraction, intersectingTasksSolids);
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
        OpeningRealKr opening,
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
