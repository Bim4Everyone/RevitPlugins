using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels;

internal class SelectionModeViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private List<SpotDimensionTypeViewModel> _spotDimentionTypes;

    public SelectionModeViewModel(RevitRepository revitRepository, ISpotDimensionSelection selection, string description) {
        _revitRepository = revitRepository;

        Description = description;
        Selection = selection;

        GetSpotDimensionTypes(null);
        GetSpotDimensionTypesCommand = new RelayCommand(GetSpotDimensionTypes);
    }

    public string Description { get; set; }
    public ISpotDimensionSelection Selection { get; set; }
    public ICommand GetSpotDimensionTypesCommand { get; set; }

    public List<SpotDimensionTypeViewModel> SpotDimentionTypes {
        get => _spotDimentionTypes;
        set => RaiseAndSetIfChanged(ref _spotDimentionTypes, value);
    }

    public IEnumerable<SpotDimension> GetSpotDimensions() {
        return _revitRepository.GetSpotDimensions(Selection);
    }

    private void GetSpotDimensionTypes(object p) {
        SpotDimentionTypes = _revitRepository
            .GetSpotDimentionTypeNames(Selection)
            .Select(item => new SpotDimensionTypeViewModel(item))
            .ToList();
    }
}
