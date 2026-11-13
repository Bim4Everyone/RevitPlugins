using System;
using System.Threading;

using dosymep.SimpleServices;

using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Services;

internal class ProgressService(ILocalizationService localizationService) {
    private int _total;
    private int _processed;
    private int _reported;
    private int _maxPercent = 99;
    
    public CancellationToken CancellationToken { get; set; }
    public IProgress<int> ProgressCount { get; set; }
    public Action<string, int, int> SetupStage { get; set; }
    public string AreaPlanNumber { get; set; } = string.Empty;
    public string AreaPlanCount { get; set; } = string.Empty;
    

    public void BeginStage(ProgressType progressType, int max = 100, int step = 5) {
        SetupStage?.Invoke(GetProgressName(progressType), max, step);
        ProgressCount?.Report(0);
    }
    
    /// <summary>
    /// Инициализация счетчиков для цикла обработки.
    /// </summary>
    public void BeginItemsProgress(int total, int maxPercent = 99) {
        _total = Math.Max(total, 0);
        _processed = 0;
        _reported = 0;
        _maxPercent = Math.Min(99, Math.Max(1, maxPercent));
    }

    /// <summary>
    /// Увеличивает processed на 1 и репортит прогресс, если процент вырос.
    /// </summary>
    public void ReportNextItem() {
        if(_total <= 0) {
            return;
        }

        _processed++;
        int current = _processed * _maxPercent / _total;
        if(current > _maxPercent) {
            current = _maxPercent;
        }

        if(current <= _reported) {
            return;
        }

        _reported = current;
        ProgressCount?.Report(_reported);
    }

    public void ThrowIfCancellationRequested() {
        CancellationToken.ThrowIfCancellationRequested();
    }

    private string GetProgressName(ProgressType progressType) {
        string progressTitle = localizationService.GetLocalizedString($"ProgressTitle.{progressType}");
        string zoneName = localizationService.GetLocalizedString("BoundaryProcessor.AreaPlanName", AreaPlanNumber, AreaPlanCount);
        return string.Join(" ", zoneName, progressTitle);
    }
}
