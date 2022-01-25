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
        private ObservableCollection<IConditionViewModel> _conditions;

        public ObservableCollection<IConditionViewModel> Conditions {
            get => _conditions;
            set => this.RaiseAndSetIfChanged(ref _conditions, value);
        }
    }
}
