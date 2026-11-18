using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.Models;

public class RevitFamily {
    private Lazy<(IList<RevitParam> Parameters, IList<RevitParam> InstanceParameters)> _parameters;

    public Family Family { get; set; }
    public IEnumerable<FamilySymbol> FamilySymbols { get; set; }
    public IEnumerable<FamilyInstance> FamilyInstances { get; set; }
    public IEnumerable<RevitParam> OrderedParams => _parameters?.Value.Parameters ?? [];
    public IEnumerable<RevitParam> InstanceParams => _parameters?.Value.InstanceParameters ?? [];

    internal void SetParameterProvider(
        Func<(IList<RevitParam> Parameters, IList<RevitParam> InstanceParameters)> provider) {
        _parameters = new Lazy<(IList<RevitParam>, IList<RevitParam>)>(provider);
    }
}
