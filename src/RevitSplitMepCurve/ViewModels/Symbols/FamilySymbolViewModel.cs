using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSplitMepCurve.ViewModels.Symbols;

internal class FamilySymbolViewModel : BaseViewModel, IEquatable<FamilySymbolViewModel> {
    public FamilySymbolViewModel(FamilySymbol symbol) {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Name = $"{symbol.FamilyName} / {symbol.Name}";
    }

    public string Name { get; }

    public FamilySymbol Symbol { get; }

    public bool Equals(FamilySymbolViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }
        return Symbol.Id == other.Symbol.Id;
    }

    public override bool Equals(object obj) => Equals(obj as FamilySymbolViewModel);

    public override int GetHashCode() =>
        EqualityComparer<ElementId>.Default.GetHashCode(Symbol.Id);
}
