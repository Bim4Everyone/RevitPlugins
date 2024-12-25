using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCopyInteriorSpecs.Models;

namespace RevitCopyInteriorSpecs.ViewModels.MainViewModelParts {
    internal class SelectedSpecsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private List<ViewSchedule> _selectedSpecs;

        public SelectedSpecsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            SelectedSpecs = _revitRepository.GetSelectedSpecs();
        }

        public List<ViewSchedule> SelectedSpecs {
            get => _selectedSpecs;
            set => this.RaiseAndSetIfChanged(ref _selectedSpecs, value);
        }
    }
}
