using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Settings;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels.Symbols;

namespace RevitSplitMepCurve.ViewModels.Providers;

internal class DuctsProviderViewModel : ElementsProviderViewModel {
    public DuctsProviderViewModel(
        ILocalizationService localization,
        DuctsProvider provider,
        RevitRepository revitRepository) : base(localization, provider) {
        var roundSymbols = revitRepository
            .GetConnectorSymbols(BuiltInCategory.OST_DuctFitting, ConnectorProfileType.Round)
            .Select(s => new FamilySymbolViewModel(s))
            .ToArray();
        var rectangleSymbols = revitRepository
            .GetConnectorSymbols(BuiltInCategory.OST_DuctFitting, ConnectorProfileType.Rectangular)
            .Select(s => new FamilySymbolViewModel(s))
            .ToArray();
        RoundSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRound"),
            roundSymbols);
        RectangleSymbol = new SelectableFamilySymbolViewModel(
            localization.GetLocalizedString("MainWindow.ConnectorRectangle"),
            rectangleSymbols);
        Name = _localization.GetLocalizedString(
            $"{nameof(RevitSplitMepCurve.Models.Enums.MepClass)}.{provider.MepClass}");
    }

    public override string Name { get; }

    public override MepClass MepClass => MepClass.Ducts;

    public SelectableFamilySymbolViewModel RoundSymbol { get; }

    public SelectableFamilySymbolViewModel RectangleSymbol { get; }

    public override string GetErrorText() {
        if(RoundSymbol.SelectedItem is null || RectangleSymbol.SelectedItem is null) {
            return _localization.GetLocalizedString("MainWindow.Validation.NoConnector");
        }
        return null;
    }

    public override ISplitSettings GetSplitSettings(ICollection<Level> levels) {
        if(RoundSymbol.SelectedItem is null
           || RectangleSymbol.SelectedItem is null) {
            throw new InvalidOperationException();
        }
        return new SplitSettings(
            RoundSymbol.SelectedItem.Symbol,
            RectangleSymbol.SelectedItem.Symbol,
            levels);
    }
}
