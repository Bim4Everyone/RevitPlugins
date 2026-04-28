using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels.Symbols;

namespace RevitSplitMepCurve.ViewModels.Providers;

internal class DuctsProviderViewModel : ElementsProviderViewModel, IEquatable<DuctsProviderViewModel> {
    public DuctsProviderViewModel(
        ILocalizationService localization,
        DuctsProvider provider,
        RevitRepository revitRepository) : base(localization, provider) {
        var symbols = revitRepository
            .GetConnectorSymbols(MepClass.Ducts)
            .Select(s => new FamilySymbolViewModel(s))
            .ToArray();
        RoundSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRound"), symbols);
        RectangleSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRectangle"), symbols);
    }

    public override string Name =>
        _localization.GetLocalizedString("MainWindow.MepClass.Ducts");

    public override MepClass MepClass => MepClass.Ducts;

    public SelectableFamilySymbolViewModel RoundSymbol { get; }

    public SelectableFamilySymbolViewModel RectangleSymbol { get; }

    public override string GetErrorText() {
        if(RoundSymbol.SelectedItem is null || RectangleSymbol.SelectedItem is null) {
            return _localization.GetLocalizedString("MainWindow.Validation.NoConnector");
        }
        return null;
    }

    public override (FamilySymbol Round, FamilySymbol Rectangle) GetSelectedSymbols() =>
        (RoundSymbol.SelectedItem?.Symbol, RectangleSymbol.SelectedItem?.Symbol);

    public bool Equals(DuctsProviderViewModel other) => other is not null;

    public override bool Equals(object obj) => Equals(obj as DuctsProviderViewModel);

    public override int GetHashCode() => GetType().GetHashCode();
}
