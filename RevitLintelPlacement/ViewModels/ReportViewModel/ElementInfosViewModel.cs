using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfosViewModel : BaseViewModel {
        private ObservableCollection<ElementInfoViewModel> _elementIfos;

        public ObservableCollection<ElementInfoViewModel> ElementIfos { 
            get => _elementIfos; 
            set => this.RaiseAndSetIfChanged(ref _elementIfos, value); 
        }
    }
}
