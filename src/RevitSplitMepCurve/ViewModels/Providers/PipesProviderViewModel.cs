using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels.Symbols;

namespace RevitSplitMepCurve.ViewModels.Providers;

internal class PipesProviderViewModel : ElementsProviderViewModel {
    public PipesProviderViewModel(
        ILocalizationService localization,
        PipesProvider provider,
        RevitRepository revitRepository) : base(localization, provider) {
        var symbols = revitRepository
            .GetConnectorSymbols(MepClass.Pipes)
            .Select(s => new FamilySymbolViewModel(s))
            .ToArray();
        RoundSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRound"), symbols);
    }

    public override string Name =>
        _localization.GetLocalizedString("MainWindow.MepClass.Pipes");

    public override MepClass MepClass => MepClass.Pipes;

    public SelectableFamilySymbolViewModel RoundSymbol { get; }

    public override string GetErrorText() {
        if(RoundSymbol.SelectedItem is null) {
            return _localization.GetLocalizedString("MainWindow.Validation.NoConnector");
        }
        return null;
    }

    public override (FamilySymbol Round, FamilySymbol Rectangle) GetSelectedSymbols() =>
        (RoundSymbol.SelectedItem?.Symbol, null);
}
