using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class EvaluatorViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    public EvaluatorViewModel(ILocalizationService localizationService, SetEvaluator setEvaluator) {
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        SetEvaluator = setEvaluator
            ?? throw new System.ArgumentNullException(nameof(setEvaluator));
        Name = _localizationService.GetLocalizedString($"{nameof(SetEvaluators)}.{SetEvaluator.Evaluator}");
    }


    public string Name { get; }

    public SetEvaluator SetEvaluator { get; }
}
