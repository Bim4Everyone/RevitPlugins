using System;
using System.Threading;

using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Services;
internal class ProgressService {
    private readonly ILocalizationService _localizationService;

    public ProgressService(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public CancellationToken CancellationToken { get; set; }
    public IProgress<int> ProgressCount { get; set; }
    public string ZoneNumber { get; set; } = string.Empty;
    public string ZoneCount { get; set; } = string.Empty;
    public Action<string, int, int> SetupStage { get; set; }

    public void BeginStage(ProgressType progressType, int max = 100, int step = 5) {
        SetupStage?.Invoke(GetProgressName(progressType), max, step);
        ProgressCount?.Report(0);
    }

    public string GetProgressName(ProgressType progressType) {
        string progressTitle = _localizationService.GetLocalizedString($"{progressType}.ProgressTitle");
        string zoneName = _localizationService.GetLocalizedString("BuildCoordVolumesProcessor.ZoneName", ZoneNumber, ZoneCount);
        return string.Join(" ", zoneName, progressTitle);
    }
}
