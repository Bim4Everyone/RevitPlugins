using System;
using System.Windows.Data;

using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Results;

/// <summary>
/// Вью модель для выбранного свойства в уровне группировки
/// </summary>
internal class PropertyViewModel : BaseViewModel, IEquatable<PropertyViewModel> {
    private readonly string _propertyName;

    public PropertyViewModel(string displayName, string propertyName) {
        if(string.IsNullOrWhiteSpace(displayName)) {
            throw new ArgumentException(nameof(displayName));
        }

        if(string.IsNullOrWhiteSpace(propertyName)) {
            throw new ArgumentException(nameof(propertyName));
        }

        DisplayName = displayName;
        _propertyName = propertyName;
        PropertyGroupDescription = new PropertyGroupDescription(_propertyName);
    }

    public string DisplayName { get; }

    public PropertyGroupDescription PropertyGroupDescription { get; }

    public bool Equals(PropertyViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _propertyName == other._propertyName && DisplayName == other.DisplayName;
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

        return Equals((PropertyViewModel) obj);
    }

    public override int GetHashCode() {
        unchecked {
            return ((_propertyName != null ? _propertyName.GetHashCode() : 0) * 397)
                   ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
        }
    }
}
