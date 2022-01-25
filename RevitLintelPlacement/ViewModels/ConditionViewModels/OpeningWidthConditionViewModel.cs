using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class OpeningWidthConditionViewModel : BaseViewModel, IConditionViewModel {
        private double _minWidth;
        private double _maxWidth;

        public double MinWidth {
            get => _minWidth;
            set => this.RaiseAndSetIfChanged(ref _minWidth, value);
        }

        public double MaxWidth {
            get => _maxWidth;
            set => this.RaiseAndSetIfChanged(ref _maxWidth, value);
        }

    }
}
