using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class GenericModelFamilyViewModel : BaseViewModel {
        private bool _isChecked;
        private string name;

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public string Name { 
            get => name; 
            set => this.RaiseAndSetIfChanged(ref name, value); 
        }
    }
}
