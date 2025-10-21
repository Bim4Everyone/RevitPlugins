using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class EvaluatorViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private SetEvaluator _setEvaluator;
    private string _name;


    public EvaluatorViewModel(ILocalizationService localization) {
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        _name = "asdasdasda";
    }


    public SetEvaluator SetEvaluator {
        get => _setEvaluator;
        set {
            RaiseAndSetIfChanged(ref _setEvaluator, value);
            Name = _localization.GetLocalizedString($"{nameof(SetEvaluators)}.{value.Evaluator}");
        }
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }
}
