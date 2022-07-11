using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.OffsetViewModels {
    internal class OffsetViewModel : BaseViewModel, IOffsetViewModel {
        private double _from;
        private double _to;
        private double _offset;

        public double From {
            get => _from;
            set => this.RaiseAndSetIfChanged(ref _from, value);
        }
        public double To {
            get => _to;
            set => this.RaiseAndSetIfChanged(ref _to, value);
        }
        public double Offset { 
            get => _offset; 
            set => this.RaiseAndSetIfChanged(ref _offset, value); 
        }
    }
}
