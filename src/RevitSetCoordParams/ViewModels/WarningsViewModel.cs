using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private ObservableCollection<WarningGroupViewModel> _warningViewModels;

    public WarningsViewModel(ILocalizationService localizationService, RevitRepository revitRepository) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;

        ShowElementCommand = RelayCommand.Create<ElementId>(ShowElement);
    }

    public ICommand ShowElementCommand { get; }

    public IReadOnlyCollection<WarningElement> WarningElementsCollection { get; set; }

    public ObservableCollection<WarningGroupViewModel> WarningViewModels {
        get => _warningViewModels;
        set => RaiseAndSetIfChanged(ref _warningViewModels, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>
    public void LoadView() {
        WarningViewModels = new ObservableCollection<WarningGroupViewModel>(GetWarningGroupViewModels());
    }

    // Метод выделения элемента
    private void ShowElement(ElementId elementId) {
        _revitRepository.SetSelected(elementId);
    }

    // Метод формирования списка WarningGroupViewModel
    private IEnumerable<WarningGroupViewModel> GetWarningGroupViewModels() {
        return WarningElementsCollection
            .Where(warningElement => warningElement is not WarningNotFoundParamElement)
            .GroupBy(element => element.WarningType)
            .Select(group => new WarningGroupViewModel {
                Caption = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}"),
                Description = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}Description"),
                WarningElements = group.ToList(),
                ShowElementCommand = ShowElementCommand
            })
            .Concat(
                WarningElementsCollection
            .OfType<WarningNotFoundParamElement>()
            .GroupBy(w => w.RevitElement.Element.Id)
            .Select(g => new {
                WarningElements = g.ToList(),
                ParamNames = string.Join(", ", g.Select(x => x.RevitParam.Name).Distinct())
            })
            .GroupBy(g => g.ParamNames)
            .Select(g => new WarningGroupViewModel {
                Caption = $"{_localizationService.GetLocalizedString("WarningsViewModel.NotFoundParam")}: {g.Key}",
                Description = _localizationService.GetLocalizedString("WarningsViewModel.NotFoundParamDescription"),
                WarningElements = g.SelectMany(x => x.WarningElements).ToList(),
                ShowElementCommand = ShowElementCommand
            }));
    }
}
