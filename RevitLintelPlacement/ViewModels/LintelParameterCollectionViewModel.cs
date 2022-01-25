using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelParameterCollectionViewModel : BaseViewModel {
        private ObservableCollection<ILintelParameterViewModel> _lintelParameters;

        ObservableCollection<ILintelParameterViewModel> LintelParameters {
            get => _lintelParameters;
            set => this.RaiseAndSetIfChanged(ref _lintelParameters, value);
        }
    }
}
