using System;

using RevitClashDetective.ViewModels.Navigator;

namespace RevitClashDetective.Services.ReportsMerging;

internal class ClashMergePair {
    private static readonly ClashIdDocComparer _clashComparer = new();

    public ClashMergePair(ClashViewModel existing, ClashViewModel importing) {
        ExistingClash = existing ?? throw new ArgumentNullException(nameof(existing));
        ImportingClash = importing ?? throw new ArgumentNullException(nameof(importing));

        if(!_clashComparer.Equals(ExistingClash, ImportingClash)) {
            throw new ArgumentException("Коллизии не соответствуют друг другу", nameof(importing));
        }
    }

    public ClashViewModel ExistingClash { get; }
    public ClashViewModel ImportingClash { get; }
}
