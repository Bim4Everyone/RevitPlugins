using System.Collections.Generic;

namespace RevitParamsChecker.Models.Results;

internal class CheckResultsRepository {
    private readonly Dictionary<string, CheckResult> _results;

    public CheckResultsRepository() {
        _results = new();
    }

    public ICollection<CheckResult> GetCheckResults() {
        return _results.Values;
    }

    public void AddCheckResult(CheckResult checkResult) {
        _results[checkResult.CheckCopy.Name] = checkResult;
    }

    public void RemoveCheckResult(CheckResult checkResult) {
        _results.Remove(checkResult.CheckCopy.Name);
    }
}
