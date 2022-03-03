using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class SelectionModeViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private List<string> _spotDimentionTypes;

        public SelectionModeViewModel(RevitRepository revitRepository, ISelectionMode selectionMode, string description) {
            this._revitRepository = revitRepository;
            SelectionMode = selectionMode;
            Description = description;
            GetSpotDimentionTypesCommand = new RelayCommand(GetSpotDimentionTypes);
            PlaceAnnotationCommand = new RelayCommand(PlaceAnnotaions);
            GetSpotDimentionTypes(null);
        }
        public string Description { get; set; }
        public List<string> SpotDimentionTypes { 
            get => _spotDimentionTypes; 
            set => this.RaiseAndSetIfChanged(ref _spotDimentionTypes, value); 
        }
        public ISelectionMode SelectionMode { get; set; }
        public ICommand GetSpotDimentionTypesCommand { get; set; }
        public ICommand PlaceAnnotationCommand { get; set; }
        private void GetSpotDimentionTypes(object p) {
            SpotDimentionTypes = _revitRepository
                .GetSpotDimentionTypeNames(SelectionMode)
                .ToList();
        }
        private void PlaceAnnotaions(object p) {

        }
    }
}
