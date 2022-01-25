using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class NumberLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private string _name;
        private double _value;

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
