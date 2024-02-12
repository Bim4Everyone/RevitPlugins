using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class InfoElementsViewModel : BaseViewModel {
        private ObservableCollection<InfoElementViewModel> _infoElements;

        public InfoElementsViewModel() {
            InfoElements = new ObservableCollection<InfoElementViewModel>();
        }

        public ObservableCollection<InfoElementViewModel> InfoElements { 
            get => _infoElements; 
            set => this.RaiseAndSetIfChanged(ref _infoElements, value); 
        }
    }
}
