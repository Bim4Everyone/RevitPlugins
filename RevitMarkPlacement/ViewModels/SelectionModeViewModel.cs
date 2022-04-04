using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class SelectionModeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private List<SpotDimensionTypeViewModel> _spotDimentionTypes;

        public SelectionModeViewModel(RevitRepository revitRepository, ISelectionMode selectionMode, string description) {
            _revitRepository = revitRepository;
            
            Description = description;
            SelectionMode = selectionMode;
            
            GetSpotDimensionTypes(null);
            GetSpotDimensionTypesCommand = new RelayCommand(GetSpotDimensionTypes);
        }
        
        public string Description { get; set; }
        public ISelectionMode SelectionMode { get; set; }
        public ICommand GetSpotDimensionTypesCommand { get; set; }

        public List<SpotDimensionTypeViewModel> SpotDimentionTypes {
            get => _spotDimentionTypes;
            set => this.RaiseAndSetIfChanged(ref _spotDimentionTypes, value);
        }


        public IEnumerable<SpotDimension> GetSpotDimensions() => _revitRepository.GetSpotDimensions(SelectionMode);

        private void GetSpotDimensionTypes(object p) {
            SpotDimentionTypes = _revitRepository
                .GetSpotDimentionTypeNames(SelectionMode)
                .Select(item => new SpotDimensionTypeViewModel(item))
                .ToList();
        }
    }
}
