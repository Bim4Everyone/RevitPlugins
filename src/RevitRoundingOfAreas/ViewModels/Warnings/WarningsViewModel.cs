using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoundingOfAreas.Models;
using RevitRoundingOfAreas.Models.Warnings;

namespace RevitRoundingOfAreas.ViewModels.Warnings;
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
        _revitRepository.SetSelected(elementId);
    }

    // Метод формирования списка WarningGroupViewModel
    private IEnumerable<WarningGroupViewModel> GetWarningGroupViewModels() {
        var regularGroups = WarningElementsCollection
            .GroupBy(w => w.WarningType)
            .Select(group => {
                var vm = new WarningGroupViewModel {
                    Caption = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}"),
                    Description = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}Description"),
                    WarningElements = group.ToList(),
                    WarningQuantity = $"({group.Count()})",
                    ShowElementCommand = ShowElementCommand
                };
                vm.LoadView();
                return vm;
            });

        return regularGroups;
    }
}
