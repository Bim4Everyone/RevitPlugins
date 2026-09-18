using System;

using RevitParamsChecker.Models.Rules.ComparisonOperators;

namespace RevitParamsChecker.Models.Rules;

/// <summary>
/// Условие правила, которое элемент не прошел
/// </summary>
internal class ParameterFailure {
    public ParameterFailure(
        string parameterName,
        string actualValue,
        ComparisonOperator comparisonOperator,
        string expectedValue) {
        if(string.IsNullOrWhiteSpace(parameterName)) {
            throw new ArgumentException(nameof(parameterName));
        }

        ParameterName = parameterName;
        ActualValue = actualValue;
        Operator = comparisonOperator ?? throw new ArgumentNullException(nameof(comparisonOperator));
        ExpectedValue = expectedValue;
    }

    public string ParameterName { get; }

    /// <summary>
    /// Фактическое значение параметра у элемента
    /// </summary>
    public string ActualValue { get; }

    public ComparisonOperator Operator { get; }

    public string ExpectedValue { get; }
}
