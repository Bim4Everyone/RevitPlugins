using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModel {
    internal class WidthViewModel : BaseViewModel, ISizeViewModel {
        private double _value;

        public string Name { get; set; } = "Ширина";
        public double Value { 
            get => _value; 
            set => this.RaiseAndSetIfChanged(ref _value, value); 
        }
    }
}
