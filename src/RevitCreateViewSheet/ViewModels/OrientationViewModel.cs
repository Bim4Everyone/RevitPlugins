using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels;
internal class OrientationViewModel : BaseViewModel, IEquatable<OrientationViewModel> {
    private readonly ILocalizationService _localization;

    public OrientationViewModel(ILocalizationService localization, bool isBookOrientation = false) {
        _localization = localization ?? throw new System.ArgumentNullException(nameof(localization));
        IsBookOrientation = isBookOrientation;
        Name = IsBookOrientation
            ? _localization.GetLocalizedString("MainWindow.SheetFormat.Book")
            : _localization.GetLocalizedString("MainWindow.SheetFormat.Album");
    }

    public bool IsBookOrientation { get; }

    public string Name { get; }

    public bool Equals(OrientationViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }
        return IsBookOrientation == other.IsBookOrientation;
    }

    public override int GetHashCode() {
        return 919047794 + IsBookOrientation.GetHashCode();
    }

    public override bool Equals(object obj) {
        return Equals(obj as OrientationViewModel);
    }
}
