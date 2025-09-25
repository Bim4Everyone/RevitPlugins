using System;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels;

internal class WallTypeConditionViewModel : BaseViewModel, IEquatable<WallTypeConditionViewModel> {
    private string _name;

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool IsChecked { get; set; }

    public bool Equals(WallTypeConditionViewModel other) {
        return other != null
               && Name == other.Name;
    }

    public override bool Equals(object obj) {
        return Equals(obj as WallTypeConditionViewModel);
    }

    public override int GetHashCode() {
        int hashCode = 897683154;
        hashCode = hashCode * -1521134295 + Name.GetHashCode();
        return hashCode;
    }
}
