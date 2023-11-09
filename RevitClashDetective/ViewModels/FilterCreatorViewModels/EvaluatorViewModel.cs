using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class EvaluatorViewModel : BaseViewModel {
        private SetEvaluator _setEvaluator;

        public SetEvaluator SetEvaluator {
            get => _setEvaluator;
            set => this.RaiseAndSetIfChanged(ref _setEvaluator, value);
        }
    }
}
