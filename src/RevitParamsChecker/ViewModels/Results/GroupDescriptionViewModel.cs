using System;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Results;

internal class GroupDescriptionViewModel : BaseViewModel, IEquatable<GroupDescriptionViewModel> {
    private readonly Guid _id;
    private PropertyViewModel _selectedProperty;

    public GroupDescriptionViewModel(PropertyViewModel selectedProperty) {
        _selectedProperty = selectedProperty ?? throw new ArgumentNullException(nameof(selectedProperty));
        _id = Guid.NewGuid();
    }

    public PropertyViewModel SelectedProperty {
        get => _selectedProperty;
        set => RaiseAndSetIfChanged(ref _selectedProperty, value);
    }

    public bool Equals(GroupDescriptionViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _id.Equals(other._id);
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((GroupDescriptionViewModel) obj);
    }

    public override int GetHashCode() {
        return _id.GetHashCode();
    }
}
