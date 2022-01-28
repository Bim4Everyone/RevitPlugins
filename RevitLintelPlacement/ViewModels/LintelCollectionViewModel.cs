using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelCollectionViewModel : BaseViewModel {
        private ObservableCollection<LintelInfoViewModel> _lintelInfos;

        public LintelCollectionViewModel() {

            LintelInfos = new ObservableCollection<LintelInfoViewModel>();

        }

        public ObservableCollection<LintelInfoViewModel> LintelInfos { 
            get => _lintelInfos; 
            set => this.RaiseAndSetIfChanged(ref _lintelInfos, value);
        }
    }
}
