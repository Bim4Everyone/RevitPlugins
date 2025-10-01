using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class EvaluatorViewModel : BaseViewModel {
    private SetEvaluator _setEvaluator;

    public SetEvaluator SetEvaluator {
        get => _setEvaluator;
        set => RaiseAndSetIfChanged(ref _setEvaluator, value);
    }
}
