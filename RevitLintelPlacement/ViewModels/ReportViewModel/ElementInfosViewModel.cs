using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfosViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<ElementInfoViewModel> _elementIfos;
        private ViewOrientation3D _orientation;

        public ElementInfosViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            ElementIfos = new ObservableCollection<ElementInfoViewModel>();
            _orientation = _revitRepository.GetOrientation3D();
            SelectElementCommand = new RelayCommand(SelectElement, p => true);
        }

        public ICommand SelectElementCommand { get; set; }

        public ObservableCollection<ElementInfoViewModel> ElementIfos { 
            get => _elementIfos; 
            set => this.RaiseAndSetIfChanged(ref _elementIfos, value); 
        }

        private void SelectElement(object p) {
            if (p is ElementInfoViewModel elementInfo) {
                _revitRepository.SelectAndShowElement(elementInfo.ElementId, _orientation);
            }
        }
    }
}
