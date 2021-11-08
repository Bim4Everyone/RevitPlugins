using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels {
    internal class InfoElementsViewModel : BaseViewModel {
        public InfoElementViewModel InfoElement { get; set; }
        public ObservableCollection<InfoElementViewModel> InfoElements { get; set; }


        public string ImageSource { get; } = "pack://application:,,,/Resources/icons8-show-property-96.png";
    }
}
