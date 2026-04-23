using System;

using dosymep.SimpleServices;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashStatusViewModel : IEquatable<ClashStatusViewModel> {
    public ClashStatusViewModel(ILocalizationService localizationService, ClashStatus status) {
        if(localizationService == null) {
            throw new ArgumentNullException(nameof(localizationService));
        }

        Status = status;
        Name = localizationService.GetLocalizedString($"{nameof(ClashStatus)}.{status}");
    }

    public ClashStatus Status { get; }
    public string Name { get; }

    public bool Equals(ClashStatusViewModel other) {
        if(other is null) {
            return false;
        }

        return ReferenceEquals(this, other) || Status == other.Status;
    }

    public override bool Equals(object obj) {
        return Equals(obj as ClashStatusViewModel);
    }

    public override int GetHashCode() {
        return Status.GetHashCode();
    }
}
