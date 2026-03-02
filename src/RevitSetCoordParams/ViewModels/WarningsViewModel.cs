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
    private string _warningQuantity;

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
    public string WarningQuantity {
        get => _warningQuantity;
        set => RaiseAndSetIfChanged(ref _warningQuantity, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>
    public void LoadView() {
        WarningViewModels = new ObservableCollection<WarningGroupViewModel>(GetWarningGroupViewModels());
        WarningQuantity = $"({WarningViewModels.Count})";
    }

    // Метод выделения элемента
    private void ShowElement(ElementId elementId) {
        _revitRepository.SetSelected([elementId]);
    }

    // Метод формирования списка WarningGroupViewModel
    private IEnumerable<WarningGroupViewModel> GetWarningGroupViewModels() {
        var regularGroups = WarningElementsCollection
            .Where(w => w is not WarningNotFoundParamElement)
            .GroupBy(w => w.WarningType)
            .Select(group => {
                var vm = new WarningGroupViewModel(_revitRepository) {
                    Caption = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}"),
                    Description = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}Description"),
                    WarningElements = group.ToList(),
                    WarningQuantity = $"({group.Count()})",
                    ShowElementCommand = ShowElementCommand
                };
                vm.LoadView();
                return vm;
            });

        var notFoundGroups = WarningElementsCollection
            .OfType<WarningNotFoundParamElement>()
            .GroupBy(w => w.RevitElement.Element.Id)
            .Select(g => new {
                WarningElements = g.ToList(),
                ParamNames = string.Join(", ", g.Select(x => x.RevitParam.Name).Distinct())
            })
            .GroupBy(g => g.ParamNames)
            .Select(g => {
                var vm = new WarningGroupViewModel(_revitRepository) {
                    Caption = $"{_localizationService.GetLocalizedString("WarningsViewModel.NotFoundParam")}: {g.Key}",
                    Description = _localizationService.GetLocalizedString("WarningsViewModel.NotFoundParamDescription"),
                    WarningElements = g.SelectMany(x => x.WarningElements).ToList(),
                    WarningQuantity = $"({g.Count()})",
                    ShowElementCommand = ShowElementCommand
                };
                vm.LoadView();
                return vm;
            });
        return regularGroups.Concat(notFoundGroups);
    }
}
