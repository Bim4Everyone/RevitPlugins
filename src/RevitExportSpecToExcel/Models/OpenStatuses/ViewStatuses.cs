using dosymep.SimpleServices;

namespace RevitExportSpecToExcel.Models;

internal class ViewStatuses {
    private readonly ILocalizationService _localizationService;
    private readonly OpenStatus _activeViewStatus;
    private readonly OpenStatus _openedViewStatus;
    private readonly OpenStatus _closedViewStatus;

    public ViewStatuses(ILocalizationService localizationService) {
        _localizationService = localizationService;

        _activeViewStatus = 
            new OpenStatus(_localizationService.GetLocalizedString("MainWindow.ActiveViewStatus"), 1);
        _openedViewStatus = 
            new OpenStatus(_localizationService.GetLocalizedString("MainWindow.OpenedViewStatus"), 2);
        _closedViewStatus = 
            new OpenStatus(_localizationService.GetLocalizedString("MainWindow.ClosedViewStatus"), 3);
    }

    public OpenStatus ActiveViewStatus => _activeViewStatus;
    public OpenStatus OpenedViewStatus => _openedViewStatus;
    public OpenStatus ClosedViewStatus => _closedViewStatus;
}
