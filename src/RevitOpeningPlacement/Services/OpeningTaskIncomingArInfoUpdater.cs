using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис обновления информации по входящим заданиям на отверстия от АР в активном файле КР.
/// <para>Обслуживает все типы входящих заданий от АР: чистовые отверстия АР и вентблоки.</para>
/// </summary>
internal class OpeningTaskIncomingArInfoUpdater : OpeningTaskIncomingInfoUpdaterBase {
    public OpeningTaskIncomingArInfoUpdater(
        RevitRepository revitRepository,
        ISolidProviderUtils solidUtils,
        IIntersectingElementsFinder intersectingElementsFinder,
        IHostFinder hostFinder)
        : base(revitRepository, solidUtils, intersectingElementsFinder, hostFinder) {
        // задания от АР принимает только КР
        RealOpenings = _revitRepository.GetRealOpeningsKr().ToArray<IOpeningReal>();
    }

    private protected override ICollection<IOpeningReal> RealOpenings { get; }
}
