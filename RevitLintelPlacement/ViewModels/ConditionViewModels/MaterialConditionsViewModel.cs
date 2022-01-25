using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class MaterialConditionsViewModel : BaseViewModel, IConditionViewModel {
        private ObservableCollection<MaterialConditionViewModel> _materialConditions;

        public ObservableCollection<MaterialConditionViewModel> MaterialConditions {
            get => _materialConditions;
            set => this.RaiseAndSetIfChanged(ref _materialConditions, value);
        }
    }
}
