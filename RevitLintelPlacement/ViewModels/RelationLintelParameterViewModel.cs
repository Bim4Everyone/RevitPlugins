using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class RelationLintelParameterViewModel : BaseViewModel, ILintelParameterViewModel {
        private readonly string openingParameterValue;
        private string _name;
        private double _relationValue;
        private string _openingParameterName;

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public double RelationValue {
            get => _relationValue;
            set => this.RaiseAndSetIfChanged(ref _relationValue, value);
        }

        public string OpeningParameterName => _openingParameterName;
    }
}
