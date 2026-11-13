using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Rules.LogicalOperators;

namespace RevitParamsChecker.ViewModels.Rules;

internal class LogicalOperatorViewModel : BaseViewModel, IEquatable<LogicalOperatorViewModel> {
    private readonly ILocalizationService _localization;

    public LogicalOperatorViewModel(ILocalizationService localization, LogicalOperator logicalOperator) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Operator = logicalOperator ?? throw new ArgumentNullException(nameof(logicalOperator));
        IsAndOperator = Operator is AndOperator;
        Name = _localization.GetLocalizedString(logicalOperator.GetType().Name);
    }

    public string Name { get; }

    ///<summary>
    /// Свойство для раскраски набора по цвету
    /// </summary>
    public bool IsAndOperator { get; }

    public LogicalOperator Operator { get; }

    public bool Equals(LogicalOperatorViewModel other) {
        return other is not null && Operator.Equals(other.Operator);
    }
}
