using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels;

internal class SelectionModeViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private ObservableCollection<SpotDimensionTypeViewModel> _spotDimensionTypes;

    public SelectionModeViewModel(ISpotDimensionSelection selection, RevitRepository revitRepository) {
        Selection = selection;
        _revitRepository = revitRepository;
    }

    public ISpotDimensionSelection Selection { get; }
    public Selections Selections => Selection.Selections;

    public ObservableCollection<SpotDimensionTypeViewModel> SpotDimensionTypes {
        get => _spotDimensionTypes;
        set => RaiseAndSetIfChanged(ref _spotDimensionTypes, value);
    }

    public void LoadSpotDimensionTypes() {
        SpotDimensionTypes ??= [
            .._revitRepository
                .GetSpotDimensionTypes(Selection)
                .Select(item => new SpotDimensionTypeViewModel(item))
        ];
    }
}
