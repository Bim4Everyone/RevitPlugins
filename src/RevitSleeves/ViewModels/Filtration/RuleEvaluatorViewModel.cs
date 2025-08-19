using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitSleeves.ViewModels.Filtration;
internal class RuleEvaluatorViewModel : BaseViewModel, IEquatable<RuleEvaluatorViewModel> {
    private readonly ILocalizationService _localizationService;

    public RuleEvaluatorViewModel(ILocalizationService localizationService, RuleEvaluator ruleEvaluator) {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        RuleEvaluator = ruleEvaluator ?? throw new ArgumentNullException(nameof(ruleEvaluator));
        Name = _localizationService.GetLocalizedString($"{nameof(RuleEvaluators)}.{RuleEvaluator.Evaluator}");
    }


    public string Name { get; }

    public RuleEvaluator RuleEvaluator { get; }


    public override bool Equals(object obj) {
        return Equals(obj as RuleEvaluatorViewModel);
    }

    public bool Equals(RuleEvaluatorViewModel other) {
        return other != null
            && RuleEvaluator.Evaluator == other.RuleEvaluator.Evaluator;
    }

    public override int GetHashCode() {
        int hashCode = 1953519430;
        hashCode = hashCode * -1521134295 + EqualityComparer<RuleEvaluators>.Default.GetHashCode(
            RuleEvaluator.Evaluator);
        return hashCode;
    }
}
