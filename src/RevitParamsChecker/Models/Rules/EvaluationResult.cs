using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitParamsChecker.Models.Rules;

/// <summary>
/// Результат проверки элемента по правилу
/// </summary>
internal class EvaluationResult {
    private EvaluationResult(bool success, IList<ParameterFailure> failures) {
        Success = success;
        Failures = failures ?? [];
    }

    public bool Success { get; }

    /// <summary>
    /// Условия, которые элемент не прошел. Пустая коллекция, если <see cref="Success"/> равно true.
    /// </summary>
    public ICollection<ParameterFailure> Failures { get; }

    public static EvaluationResult CreateSuccessResult() {
        return new EvaluationResult(true, []);
    }

    public static EvaluationResult CreateUnsuccessResult(IList<ParameterFailure> failures) {
        if(failures is null) {
            throw new ArgumentNullException(nameof(failures));
        }

        return new EvaluationResult(false, failures);
    }
}
