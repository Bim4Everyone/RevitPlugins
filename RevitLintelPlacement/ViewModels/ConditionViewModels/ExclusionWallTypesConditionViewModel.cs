using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class ExclusionWallTypesConditionViewModel : BaseViewModel, IConditionViewModel {
        private ObservableCollection<WallTypeConditionViewModel> _wallTypes;

        public ObservableCollection<WallTypeConditionViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

    }
}
