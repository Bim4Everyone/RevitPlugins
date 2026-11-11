using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitEnumerateBySpline.Models;
using RevitEnumerateBySpline.Models.Warnings;

namespace RevitEnumerateBySpline.ViewModels.Warnings;

internal sealed class WarningsViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;

    public WarningsViewModel(ILocalizationService localizationService, RevitRepository revitRepository) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
    }

    public ICommand ShowElementCommand { get; }
    public int WarningQuantity { get; private set; }
    public IReadOnlyList<WarningGroupViewModel> WarningViewModels { get; private set; } = [];

    /// <summary>Группирует предупреждения для отображения.</summary>
    public void LoadView(IReadOnlyCollection<WarningElement> warnings) {
        WarningQuantity = warnings.Count;
        WarningViewModels = warnings
            .GroupBy(warning => warning.WarningType)
            .Select(group => new WarningGroupViewModel {
                Caption = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}"),
                Description = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}Description"),
                WarningQuantity = group.Count(),
                Warnings = group.Select(warning => new WarningElementViewModel(warning.SpatialModel)).ToList()
            })
            .ToList();
    }

    // Выделяет помещение в Revit.
    private void ShowElement(ElementId elementId) {
        _revitRepository.SetSelected(elementId);
    }
}
