using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RelationLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private double _relationValue;
        private string _name;
        private string _openingParameterName;

        public double RelationValue {
            get => _relationValue;
            set => this.RaiseAndSetIfChanged(ref _relationValue, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string OpeningParameterName { 
            get => _openingParameterName; 
            private set => _openingParameterName = value; 
        }
    }
}
