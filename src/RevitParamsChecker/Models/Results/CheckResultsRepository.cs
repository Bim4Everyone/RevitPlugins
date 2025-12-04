using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitParamsChecker.Models.Results;

internal class CheckResultsRepository {
    private readonly Dictionary<string, CheckResult> _results;

    public CheckResultsRepository() {
        _results = new();
    }

    public event EventHandler<ResultsChangedEventArgs> ResultsAdded;

    public ICollection<CheckResult> GetCheckResults() {
        return _results.Values;
    }

    public void AddCheckResult(CheckResult checkResult) {
        var oldResults = GetResultsStamp();
        _results[checkResult.CheckCopy.Name] = checkResult;
        var newResults = GetResultsStamp();
        ResultsAdded?.Invoke(this, new ResultsChangedEventArgs(oldResults, newResults));
    }

    public void RemoveCheckResult(CheckResult checkResult) {
        _results.Remove(checkResult.CheckCopy.Name);
    }

    private IReadOnlyCollection<CheckResult> GetResultsStamp() {
        return new ReadOnlyCollection<CheckResult>(_results.Values.ToArray());
    }
}
