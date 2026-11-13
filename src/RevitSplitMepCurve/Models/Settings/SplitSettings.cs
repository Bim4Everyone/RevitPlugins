using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.Models.Settings;

internal class SplitSettings : ISplitSettings {
    public SplitSettings(FamilySymbol round, FamilySymbol rectangle, ICollection<Level> levels) {
        ConnectorRoundSymbol = round;
        ConnectorRectangleSymbol = rectangle;
        Levels = levels ?? [];
    }

    public FamilySymbol ConnectorRoundSymbol { get; }

    public FamilySymbol ConnectorRectangleSymbol { get; }

    public ICollection<Level> Levels { get; }
}
