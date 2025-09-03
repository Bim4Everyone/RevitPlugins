using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;
internal class ElementViewModel : BaseViewModel, IEquatable<ElementViewModel> {
    public ElementViewModel(ElementModel element, double elementVolume) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        ElementVolume = elementVolume;
    }


    public ElementModel Element { get; }

    public double ElementVolume { get; }


    public bool Equals(ElementViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return Element.Equals(other.Element);
    }

    public override int GetHashCode() {
        return -703426257 + EqualityComparer<ElementModel>.Default.GetHashCode(Element);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ElementViewModel);
    }
}
