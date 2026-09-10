using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис обновления информации по входящим заданиям на отверстия от ВИС.
/// <para>Используется в навигаторах АР и КР, чистовые отверстия берутся по разделу активного файла.</para>
/// </summary>
internal class OpeningTaskIncomingMepInfoUpdater : OpeningTaskIncomingInfoUpdaterBase {
    public OpeningTaskIncomingMepInfoUpdater(
        RevitRepository revitRepository,
        IDocTypesHandler docTypesHandler,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder,
        IHostFinder hostFinder)
        : base(revitRepository, solidUtils, intersectingElementsFinder, hostFinder) {
        if(docTypesHandler is null) {
            throw new ArgumentNullException(nameof(docTypesHandler));
        }

        RealOpenings = docTypesHandler.GetDocType(_revitRepository.Doc) == DocTypeEnum.KR
            ? _revitRepository.GetRealOpeningsKr().ToArray<IOpeningReal>()
            : _revitRepository.GetRealOpeningsAr().ToArray<IOpeningReal>();
    }

    private protected override ICollection<IOpeningReal> RealOpenings { get; }
}
