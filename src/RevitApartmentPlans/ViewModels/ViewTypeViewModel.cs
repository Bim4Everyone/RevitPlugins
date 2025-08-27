using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitApartmentPlans.ViewModels;

internal class ViewTypeViewModel : BaseViewModel, IEquatable<ViewTypeViewModel> {
    private readonly ILocalizationService _localization;

    public ViewTypeViewModel(ViewType viewType, ILocalizationService localization) {
        ViewType = viewType;
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        Name = _localization.GetLocalizedString($"{nameof(ViewType)}.{viewType}");
    }


    public ViewType ViewType { get; }

    public string Name { get; }


    public bool Equals(ViewTypeViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return ViewType == other.ViewType;
    }

    public override int GetHashCode() {
        return -1388979728 + ViewType.GetHashCode();
    }

    public override bool Equals(object obj) {
        return Equals(obj as ViewTypeViewModel);
    }
}
