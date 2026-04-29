using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Settings;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels.Symbols;

namespace RevitSplitMepCurve.ViewModels.Providers;

internal class PipesProviderViewModel : ElementsProviderViewModel {
    public PipesProviderViewModel(
        ILocalizationService localization,
        PipesProvider provider,
        RevitRepository revitRepository) : base(localization, provider) {
        var symbols = revitRepository
            .GetConnectorSymbols(BuiltInCategory.OST_PipeCurves)
            .Select(s => new FamilySymbolViewModel(s))
            .ToArray();
        RoundSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRound"), symbols);
        Name = _localization.GetLocalizedString(
            $"{nameof(RevitSplitMepCurve.Models.Enums.MepClass)}.{provider.MepClass}");
    }

    public override string Name { get; }

    public override MepClass MepClass => MepClass.Pipes;

    public SelectableFamilySymbolViewModel RoundSymbol { get; }

    public override string GetErrorText() {
        if(RoundSymbol.SelectedItem is null) {
            return _localization.GetLocalizedString("MainWindow.Validation.NoConnector");
        }
        return null;
    }

    public override ISplitSettings GetSplitSettings(ICollection<Level> levels) {
        if(RoundSymbol.SelectedItem is null) {
            throw new InvalidOperationException();
        }

        return new SplitSettings(RoundSymbol.SelectedItem.Symbol, null, levels);
    }
}
