using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningsViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IReadOnlyCollection<WarningElement> _warningElements;

    private ObservableCollection<WarningGroupViewModel> _warningViewModels;

    public WarningsViewModel(ILocalizationService localizationService, IReadOnlyCollection<WarningElement> warningElements) {
        _localizationService = localizationService;
        _warningElements = warningElements;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    public ICommand LoadViewCommand { get; }

    public ObservableCollection<WarningGroupViewModel> WarningViewModels {
        get => _warningViewModels;
        set => RaiseAndSetIfChanged(ref _warningViewModels, value);
    }


    // Метод загрузки вида
    private void LoadView() {
        WarningViewModels = new ObservableCollection<WarningGroupViewModel>(GetWarningGroupViewModels());
    }

    private IEnumerable<WarningGroupViewModel> GetWarningGroupViewModels() {
        return _warningElements
            .Where(warningElement => warningElement is not WarningNotFoundParamElement)
            .GroupBy(element => element.WarningType)
            .Select(group => new WarningGroupViewModel {
                Caption = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}"),
                Description = _localizationService.GetLocalizedString($"WarningsViewModel.{group.Key}Description"),
                WarningElements = group.ToList()
            })
            .Concat(
                _warningElements
            .OfType<WarningNotFoundParamElement>()
            .GroupBy(w => w.RevitElement.Element.Id)
            .Select(g => new {
                WarningElements = g.ToList(),
                ParamNames = string.Join(", ", g.Select(x => x.RevitParam.Name))
            })
            .GroupBy(g => g.ParamNames)
            .Select(g => new WarningGroupViewModel {
                Caption = $"{_localizationService.GetLocalizedString("WarningsViewModel.NotFoundParam")}: {g.Key}",
                Description = _localizationService.GetLocalizedString("WarningsViewModel.NotFoundParamDescription"),
                WarningElements = g.SelectMany(x => x.WarningElements).ToList()
            }));
    }
}
