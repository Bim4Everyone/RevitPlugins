using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitClassifierParameters.Models.MaterialClassifier;

namespace RevitClassifierParameters.ViewModels;

internal class ReportVM : BaseViewModel {
    private readonly MaterialReportService _materialReportService;

    private ObservableCollection<MaterialReportItem> _reportItems = [];

    public ReportVM(MaterialReportService materialReportService) {
        _materialReportService = materialReportService;
    }

    public ObservableCollection<MaterialReportItem> ReportItems {
        get => _reportItems;
        set => RaiseAndSetIfChanged(ref _reportItems, value);
    }

    /// <summary>
    /// Заполняет отчёт из собранных записей.
    /// Порядок групп повторяет Python-плагин: ошибки, изменённые, без изменений,
    /// без кода работы, ненайденный код. Внутри группы — сортировка по коду работы.
    /// </summary>
    public void UpdateReportData() {
        var ordered = _materialReportService.Items
            .OrderBy(GetStatusOrder)
            .ThenBy(item => item.WorkCode);

        ReportItems = new ObservableCollection<MaterialReportItem>(ordered);
    }

    private static int GetStatusOrder(MaterialReportItem item) {
        return item.Status switch {
            MaterialReportStatus.Error => 0,
            MaterialReportStatus.Edited => 1,
            MaterialReportStatus.NotEdited => 2,
            MaterialReportStatus.NoWorkCode => 3,
            MaterialReportStatus.ClassifierCodeNotFound => 4,
            _ => 5
        };
    }
}
