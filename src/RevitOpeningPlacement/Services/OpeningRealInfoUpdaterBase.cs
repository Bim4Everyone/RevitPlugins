using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Базовый класс сервиса обновления информации по чистовым отверстиям.
/// <para>
/// Содержит общий алгоритм определения статуса чистового отверстия относительно связей ВИС
/// и перевод доли пересеченного объема в статус. Пороги долей задаются наследниками.
/// </para>
/// </summary>
/// <typeparam name="T">Чистовое отверстие из активного файла</typeparam>
internal abstract class OpeningRealInfoUpdaterBase<T> : OpeningInfoUpdaterBase<T> where T : class, IOpeningReal {
    private protected readonly ISolidProviderUtils _solidUtils;
    private protected readonly IIntersectingElementsFinder _intersectingElementsFinder;

    protected OpeningRealInfoUpdaterBase(
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder) {
        _solidUtils = solidUtils ?? throw new ArgumentNullException(nameof(solidUtils));
        _intersectingElementsFinder = intersectingElementsFinder
                                      ?? throw new ArgumentNullException(nameof(intersectingElementsFinder));
    }

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается пустым
    /// </summary>
    private protected abstract double EmptyVolumeRatio { get; }

    /// <summary>
    /// Доля пересеченного объема, ниже которой отверстие считается слишком большим
    /// </summary>
    private protected abstract double TooBigVolumeRatio { get; }

    /// <summary>
    /// Назначает статус чистовому отверстию
    /// </summary>
    private protected abstract void SetStatus(T opening, OpeningRealStatus status);

    private protected override void SetInvalidStatus(T opening) {
        SetStatus(opening, OpeningRealStatus.Invalid);
    }

    /// <summary>
    /// Определяет статус чистового отверстия относительно элементов ВИС и заданий на отверстия из связей ВИС
    /// </summary>
    /// <param name="opening">Чистовое отверстие из активного файла</param>
    /// <param name="mepLinks">Связи с элементами ВИС и заданиями на отверстия</param>
    private protected void UpdateStatusByMepLinks(T opening, ICollection<IMepLinkElementsProvider> mepLinks) {
        var openingSolid = opening.GetSolid();
        var solidAfterIntersection = openingSolid;

        foreach(var link in mepLinks) {
            var intersectingLinkElements = GetIntersectingLinkElements(opening, link);
            if(intersectingLinkElements.Count == 0) {
                continue;
            }

            if(LinkElementsIntersectHost(opening, link, intersectingLinkElements)) {
                SetStatus(opening, OpeningRealStatus.NotActual);
                return;
            }

            solidAfterIntersection = _solidUtils.SubtractSolids(
                solidAfterIntersection,
                GetLinkElementsSolids(link, intersectingLinkElements));
        }

        SetStatus(opening, GetStatusByVolumeRatio(GetSolidsVolumesRatio(openingSolid, solidAfterIntersection)));
    }

    /// <summary>
    /// Возвращает коэффициент, показывающий, какую часть исходного солида пересекают элементы из связей
    /// </summary>
    /// <param name="openingSolid">Исходный солид текущего чистового отверстия</param>
    /// <param name="solidAfterIntersection">
    /// Солид текущего чистового отверстия после вычитания солидов элементов из связей, которые его пересекают
    /// </param>
    /// <returns>
    /// 0 - элементы из связей на 100% пересекают солид текущего чистового отверстия,
    /// 1 - солид текущего чистового отверстия не пересекается ни с одним элементом из связи
    /// </returns>
    private protected double GetSolidsVolumesRatio(Solid openingSolid, Solid solidAfterIntersection) {
        return openingSolid.Volume == 0 ? 1 : 1 - solidAfterIntersection.Volume / openingSolid.Volume;
    }

    /// <summary>
    /// Возвращает статус чистового отверстия по коэффициенту пересекаемого объема
    /// </summary>
    /// <param name="volumeRatio">
    /// Отношение объема солида чистового отверстия, который пересекается с элементами из связей,
    /// к исходному объему этого солида
    /// </param>
    private protected OpeningRealStatus GetStatusByVolumeRatio(double volumeRatio) {
        return volumeRatio < EmptyVolumeRatio
            ? OpeningRealStatus.Empty
            : volumeRatio < TooBigVolumeRatio
                ? OpeningRealStatus.TooBig
                : OpeningRealStatus.Correct;
    }

    /// <summary>
    /// Возвращает коллекцию Id элементов ВИС и заданий на отверстия из связи,
    /// которые пересекаются с чистовым отверстием
    /// </summary>
    private ICollection<ElementId> GetIntersectingLinkElements(T opening, IMepLinkElementsProvider link) {
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
        T opening,
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
}
