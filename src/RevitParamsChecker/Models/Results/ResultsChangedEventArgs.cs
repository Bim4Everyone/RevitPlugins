using System;
using System.Collections.Generic;

namespace RevitParamsChecker.Models.Results;

internal class ResultsChangedEventArgs : EventArgs {
    public ResultsChangedEventArgs(
        IReadOnlyCollection<CheckResult> oldCheckResults,
        IReadOnlyCollection<CheckResult> newCheckResults) {
        OldCheckResults = oldCheckResults ?? throw new ArgumentNullException(nameof(oldCheckResults));
        NewCheckResults = newCheckResults ?? throw new ArgumentNullException(nameof(newCheckResults));
    }

    public IReadOnlyCollection<CheckResult> OldCheckResults { get; }
    public IReadOnlyCollection<CheckResult> NewCheckResults { get; }
}
