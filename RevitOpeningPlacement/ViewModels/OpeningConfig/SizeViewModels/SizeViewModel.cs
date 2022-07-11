using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModels {
    internal class SizeViewModel : BaseViewModel, ISizeViewModel {
        private double _value;
        private string _name;

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }
        public double Value {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
    }
}
