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
internal class OpeningRealKrInfoUpdater : OpeningInfoUpdaterBase<OpeningRealKr> {
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
    private readonly ISolidProviderUtils _solidUtils;
    private readonly RealOpeningVolumeAnalyzer _volumeAnalyzer;
    private readonly RealOpeningKrStatusesSettings _statusesSettings;

    /// <summary>
    /// Минимальное допустимое расстояние между чистовыми отверстиями КР в единицах длины Revit (футах).
    /// <para>0 - проверка выключена</para>
    /// </summary>
    private readonly double _minDistance;

    /// <summary>
    /// Включает/выключает проверку расстояния между чистовыми отверстиями (статус TooClose)
    /// </summary>
    private readonly bool _checkTooClose;

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

    public OpeningRealKrInfoUpdater(
        RevitRepository revitRepository,
        OpeningRealsKrConfig config,
        ILengthConverter lengthConverter,
        ISolidProviderUtils solidUtils,
        RealOpeningVolumeAnalyzer volumeAnalyzer) {

        if(config is null) {
            throw new ArgumentNullException(nameof(config));
        }

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _lengthConverter = lengthConverter ?? throw new ArgumentNullException(nameof(lengthConverter));
        _solidUtils = solidUtils ?? throw new ArgumentNullException(nameof(solidUtils));
        _volumeAnalyzer = volumeAnalyzer ?? throw new ArgumentNullException(nameof(volumeAnalyzer));
        _statusesSettings = config.NavigatorSettings.RealOpeningSettings;
        _placementType = config.PlacementType;
        _checkTooClose = _statusesSettings.CheckTooClose;
        _minDistance = _lengthConverter.ConvertToInternal(_statusesSettings.MinDistanceBetweenOpenings);
        _realOpenings = (_checkTooClose && (_minDistance > 0)) ? revitRepository.GetRealOpeningsKr() : [];
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

    private protected override void UpdateInfoCore(OpeningRealKr opening) {
        var analysis = _placementType switch {
            OpeningRealKrPlacementType.PlaceByAr => _volumeAnalyzer.AnalyzeArLinks(
                opening,
                _arLinks,
                _statusesSettings),
            OpeningRealKrPlacementType.PlaceByMep => _volumeAnalyzer.AnalyzeMepLinks(
                opening,
                _mepLinks,
                _statusesSettings),
            _ => throw new InvalidOperationException(
                $"Режим обработки заданий для КР: '{_placementType}' не поддерживается.")
        };

        if(_statusesSettings.CheckNotActual
           && analysis.NotActual) {
            opening.Status = OpeningRealStatus.NotActual;
            return;
        }

        var volumeStatus = GetVolumeStatus(analysis.VolumeRatio);
        if(volumeStatus != OpeningRealStatus.Correct) {
            opening.Status = volumeStatus;
            return;
        }

        opening.Status = GetProximityStatus(opening);
    }

    private protected override void SetInvalidStatus(OpeningRealKr opening) {
        opening.Status = OpeningRealStatus.Invalid;
    }

    /// <summary>
    /// Возвращает статус чистового отверстия по доле пересеченного объема.
    /// <para>
    /// Диапазоны взаимоисключающие, поэтому выключение проверки не подменяет один статус другим:
    /// все отверстия становятся корректными.
    /// </para>
    /// </summary>
    /// <param name="volumeRatio">Доля объема отверстия, пересеченная элементами из связей</param>
    private OpeningRealStatus GetVolumeStatus(double volumeRatio) {
        if(!_statusesSettings.CheckVolumeMatch) {
            return OpeningRealStatus.Correct;
        }

        if(volumeRatio < _statusesSettings.EmptyVolumeRatio) {
            return OpeningRealStatus.Empty;
        }

        return volumeRatio < _statusesSettings.TooBigVolumeRatio
            ? OpeningRealStatus.TooBig
            : OpeningRealStatus.Correct;
    }

    /// <summary>
    /// Возвращает <see cref="OpeningRealStatus.TooClose"/>, если рядом с отверстием есть другое
    /// чистовое отверстие КР, иначе <see cref="OpeningRealStatus.Correct"/>.
    /// </summary>
    private OpeningRealStatus GetProximityStatus(OpeningRealKr opening) {
        if(!_checkTooClose
           || (_minDistance <= 0)) {
            _proximityPool.Remove(opening.Id);
            return OpeningRealStatus.Correct;
        }

        var neighbourIds = _proximityPool.Where(id => id != opening.Id).ToArray();
        if(neighbourIds.Length == 0) {
            _proximityPool.Remove(opening.Id);
            return OpeningRealStatus.Correct;
        }

        // грубый отбор кандидатов по раздутому во все стороны боксу
        var candidateIds = new FilteredElementCollector(_revitRepository.Doc, neighbourIds)
            .WherePasses(new BoundingBoxIntersectsFilter(GetInflatedOutline(opening)))
            .ToElementIds();
        if(candidateIds.Count == 0) {
            _proximityPool.Remove(opening.Id);
            return OpeningRealStatus.Correct;
        }

        // точная проверка: раздувается только проверяемое отверстие и только в плоскости,
        // перпендикулярной его оси
        var inflatedSolid = opening.GetInflatedSolid(_minDistance);
        var inflatedBBox = inflatedSolid.GetTransformedBoundingBox();
        bool tooClose = _realOpenings
            .Where(neighbour => candidateIds.Contains(neighbour.Id))
            .Any(neighbour => _solidUtils.IntersectsSolid(neighbour, inflatedSolid, inflatedBBox));

        if(tooClose) {
            return OpeningRealStatus.TooClose;
        }

        _proximityPool.Remove(opening.Id);
        return OpeningRealStatus.Correct;
    }

    /// <summary>
    /// Возвращает бокс чистового отверстия, раздутый на минимальное допустимое расстояние по всем осям
    /// </summary>
    private Outline GetInflatedOutline(OpeningRealKr opening) {
        var bbox = opening.GetTransformedBBoxXYZ();
        var offset = new XYZ(_minDistance, _minDistance, _minDistance);
        return new Outline(bbox.Min - offset, bbox.Max + offset);
    }
}
