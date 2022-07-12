using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.SizeViewModels {
    internal class SizeViewModel : BaseViewModel, ISizeViewModel {
        private double _value;
        private string _name;

        public SizeViewModel(Size size) {
            Name = size.Name;
            Value = size.Value;
        }

        public SizeViewModel() {

        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public double Value {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public Size GetSize() {
            return new Size() { Name = Name, Value = Value };
        }
    }
}
