using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class LinkViewModel : BaseViewModel {
        private bool _isChecked;
        private string _name;

        public bool IsChecked { 
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value); 
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
    }
}
