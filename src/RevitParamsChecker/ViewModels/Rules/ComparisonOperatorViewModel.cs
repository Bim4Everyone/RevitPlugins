using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules.ComparisonOperators;

namespace RevitParamsChecker.ViewModels.Rules;

internal class ComparisonOperatorViewModel : BaseViewModel, IEquatable<ComparisonOperatorViewModel> {
    private readonly ILocalizationService _localization;

    public ComparisonOperatorViewModel(ILocalizationService localization, ComparisonOperator comparisonOperator) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Operator = comparisonOperator ?? throw new ArgumentNullException(nameof(comparisonOperator));
        Name = _localization.GetLocalizedString(comparisonOperator.GetType().Name);
    }

    public string Name { get; }

    public ComparisonOperator Operator { get; }

    public bool Equals(ComparisonOperatorViewModel other) {
        return other is not null && Operator.Equals(other.Operator);
    }
}
