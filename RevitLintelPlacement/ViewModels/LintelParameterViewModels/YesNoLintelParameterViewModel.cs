using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class YesNoLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private string _name;
        private bool _isChecked;

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsChecked { 
            get => _isChecked; 
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

    }
}
