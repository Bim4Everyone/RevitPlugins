using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitLintelsManager.ViewModels;

internal class FamilyViewModel : ItemCheckViewModel {
    public Family Family { get; set; }
    public IEnumerable<RevitParam> OrderedParams { get; set; }
}
