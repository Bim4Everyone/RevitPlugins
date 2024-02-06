using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels {
    internal class InfoElementsViewModel : BaseViewModel {
        private InfoElementViewModel _infoElement;

        public InfoElementViewModel InfoElement {
            get => _infoElement; 
            set => this.RaiseAndSetIfChanged(ref _infoElement, value);
        }

        public ObservableCollection<InfoElementViewModel> InfoElements { get; set; }
    }
}
