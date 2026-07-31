using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.Models;

public class RevitFamily {
    public Family Family{ get; set; }
    public IEnumerable<FamilySymbol> FamilySymbols { get; set; }
    public IEnumerable<FamilyInstance> FamilyInstances { get; set; }
    public IEnumerable<RevitParam> OrderedParams { get; set; }
}
