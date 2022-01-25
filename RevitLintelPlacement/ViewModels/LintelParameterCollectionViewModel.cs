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
        private ObservableCollection<ILintelParameterViewModel> _lintelParameterCollection;

        ObservableCollection<ILintelParameterViewModel> LintelParameterCollection {
            get => _lintelParameterCollection;
            set => this.RaiseAndSetIfChanged(ref _lintelParameterCollection, value);
        }
    }
}
