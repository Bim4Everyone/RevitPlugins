using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис обновления информации по чистовым отверстиям АР из активного файла АР
/// относительно связей ВИС
/// </summary>
internal class OpeningRealArInfoUpdater : OpeningRealInfoUpdaterBase<OpeningRealAr> {
    /// <summary>
    /// Все связи ВИС, загруженные в активный файл
    /// </summary>
    private readonly ICollection<IMepLinkElementsProvider> _mepLinks;

    public OpeningRealArInfoUpdater(
        RevitRepository revitRepository,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder)
        : base(solidUtils, intersectingElementsFinder) {

        if(revitRepository is null) {
            throw new ArgumentNullException(nameof(revitRepository));
        }
        _mepLinks = revitRepository.GetSelectedRevitLinks()
            .Select(link => new MepLinkElementsProvider(link) as IMepLinkElementsProvider)
            .ToArray();
    }


    private protected override double EmptyVolumeRatio => 0.01;

    private protected override double TooBigVolumeRatio => 0.2;


    private protected override void UpdateInfoCore(OpeningRealAr opening) {
        UpdateStatusByMepLinks(opening, _mepLinks);
    }

    private protected override void SetStatus(OpeningRealAr opening, OpeningRealStatus status) {
        opening.Status = status;
    }
}
