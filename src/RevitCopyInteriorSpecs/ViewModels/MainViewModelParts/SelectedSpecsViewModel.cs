using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCopyInteriorSpecs.Models;

namespace RevitCopyInteriorSpecs.ViewModels.MainViewModelParts;
internal class SelectedSpecsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private List<ViewSchedule> _selectedSpecs = [];

    public SelectedSpecsViewModel(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public List<ViewSchedule> SelectedSpecs {
        get => _selectedSpecs;
        set => RaiseAndSetIfChanged(ref _selectedSpecs, value);
    }

    public void GetSelectedSpecs() {
        SelectedSpecs = _revitRepository.GetSelectedSpecs();
    }
}
