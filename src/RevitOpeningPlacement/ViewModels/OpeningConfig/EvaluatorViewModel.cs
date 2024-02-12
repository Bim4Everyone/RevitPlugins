using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class EvaluatorViewModel : BaseViewModel {
        private SetEvaluator _setEvaluator;

        public SetEvaluator SetEvaluator {
            get => _setEvaluator;
            set => RaiseAndSetIfChanged(ref _setEvaluator, value);
        }
    }
}
