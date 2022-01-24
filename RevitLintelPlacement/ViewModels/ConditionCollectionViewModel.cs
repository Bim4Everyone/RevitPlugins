using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class ConditionCollectionViewModel : BaseViewModel {
        private ObservableCollection<IConditionViewModel> _conditionCollection;

        public ObservableCollection<IConditionViewModel> ConditionCollection { 
            get => _conditionCollection; 
            set => this.RaiseAndSetIfChanged(ref _conditionCollection, value); 
        }
    }
}
