using System;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.Models;

internal class ConnectorConfig {
    public string FamilyName { get; set; } = string.Empty;

    public string SymbolName { get; set; } = string.Empty;

    public bool Equals(FamilySymbol s) {
        if(s is null) {
            return false;
        }

        if(FamilyName is null
           || SymbolName is null) {
            return false;
        }

        return SymbolName.Equals(s.Name, StringComparison.CurrentCultureIgnoreCase)
               && FamilyName.Equals(s.FamilyName, StringComparison.CurrentCultureIgnoreCase);
    }
}
