using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.ViewModels;
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
            .Where(w => w is not WarningNotFilledParamElement)
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

        var notFoundGroups = WarningElementsCollection
            .OfType<WarningNotFilledParamElement>()
            .GroupBy(w => w.SpatialObject.SpatialElement.Id)
            .Select(g => new {
                g.First().SpatialObject,
                ElementId = g.First().SpatialObject.SpatialElement.Id,
                ParamNames = string.Join(", ", g.Select(x => x.RevitParam.Name).Distinct())
            })
            .GroupBy(x => x.ParamNames)
            .Select(g => {
                var vm = new WarningGroupViewModel {
                    Caption = $"{_localizationService.GetLocalizedString("WarningsViewModel.NotFilledParam")}: {g.Key}",
                    Description = _localizationService.GetLocalizedString("WarningsViewModel.NotFilledParamDescription"),
                    WarningElements = g.Select(x => new WarningNotFilledParamElement {
                        WarningType = WarningType.NotFilledParam,
                        SpatialObject = x.SpatialObject
                    }).Cast<WarningElement>().ToList(),
                    WarningQuantity = $"({g.Count()})",
                    ShowElementCommand = ShowElementCommand
                };
                vm.LoadView();
                return vm;
            });

        return regularGroups.Concat(notFoundGroups);
    }
}
