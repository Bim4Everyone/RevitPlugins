using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис обновления информации по чистовым отверстиям АР из активного файла АР
/// относительно связей ВИС
/// </summary>
internal class OpeningRealArInfoUpdater : OpeningInfoUpdaterBase<OpeningRealAr> {
    /// <summary>
    /// Все связи ВИС, загруженные в активный файл
    /// </summary>
    private readonly ICollection<IMepLinkElementsProvider> _mepLinks;

    private readonly RealOpeningVolumeAnalyzer _volumeAnalyzer;
    private readonly RealOpeningArStatusesSettings _arStatusesSettings;

    public OpeningRealArInfoUpdater(
        RevitRepository revitRepository,
        OpeningRealsArConfig config,
        RealOpeningVolumeAnalyzer volumeAnalyzer) {

        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }

        if(config is null) {
            throw new ArgumentNullException(nameof(config));
        }

        _volumeAnalyzer = volumeAnalyzer ?? throw new ArgumentNullException(nameof(volumeAnalyzer));
        _arStatusesSettings = config.NavigatorSettings.RealOpeningSettings;
        _mepLinks = revitRepository.GetSelectedRevitLinks()
            .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
            .ToArray();
    }


    private protected override void UpdateInfoCore(OpeningRealAr opening) {
        var analysis = _volumeAnalyzer.AnalyzeMepLinks(
            opening,
            _mepLinks,
            _arStatusesSettings);

        if(_arStatusesSettings.CheckNotActual
           && analysis.NotActual) {
            opening.Status = OpeningRealStatus.NotActual;
            return;
        }

        opening.Status = GetVolumeStatus(analysis.VolumeRatio);
    }

    private protected override void SetInvalidStatus(OpeningRealAr opening) {
        opening.Status = OpeningRealStatus.Invalid;
    }

    /// <summary>
    /// Возвращает статус чистового отверстия по доле пересеченного объема.
    /// </summary>
    /// <param name="volumeRatio">Доля объема отверстия, пересеченная элементами из связей</param>
    private OpeningRealStatus GetVolumeStatus(double volumeRatio) {
        if(!_arStatusesSettings.CheckVolumeMatch) {
            return OpeningRealStatus.Correct;
        }

        if(volumeRatio < _arStatusesSettings.EmptyVolumeRatio) {
            return OpeningRealStatus.Empty;
        }

        return volumeRatio < _arStatusesSettings.TooBigVolumeRatio
            ? OpeningRealStatus.TooBig
            : OpeningRealStatus.Correct;
    }
}
