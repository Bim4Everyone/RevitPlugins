using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class WallTypesConditionViewModel : BaseViewModel, IConditionViewModel {
        private ObservableCollection<WallTypeConditionViewModel> _wallTypes;

        ObservableCollection<WallTypeConditionViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

    }

}
