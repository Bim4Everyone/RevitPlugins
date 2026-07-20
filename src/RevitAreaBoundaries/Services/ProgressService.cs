using System;
using System.Threading;

using dosymep.SimpleServices;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Services;

internal class ProgressService(ILocalizationService localizationService) {
    
    public CancellationToken CancellationToken { get; set; }
    public IProgress<int> ProgressCount { get; set; }
    public Action<string, int, int> SetupStage { get; set; }
    public string AreaPlanNumber { get; set; } = string.Empty;
    public string AreaPlanCount { get; set; } = string.Empty;
    

    public void BeginStage(ProgressType progressType, int max = 100, int step = 5) {
        SetupStage?.Invoke(GetProgressName(progressType), max, step);
        ProgressCount?.Report(0);
    }

    public string GetProgressName(ProgressType progressType) {
        string progressTitle = localizationService.GetLocalizedString($"ProgressTitle.{progressType}");
        string zoneName = localizationService.GetLocalizedString("BoundaryProcessor.AreaPlanName", AreaPlanNumber, AreaPlanCount);
        return string.Join(" ", zoneName, progressTitle);
    }
}
