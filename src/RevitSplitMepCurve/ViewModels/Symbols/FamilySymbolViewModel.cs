using System;

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

    public bool Equals(FamilySymbolViewModel other) =>
        other is not null && Symbol.Id == other.Symbol.Id;

    public override bool Equals(object obj) => Equals(obj as FamilySymbolViewModel);

    public override int GetHashCode() => (int)Symbol.Id.Value;
}
