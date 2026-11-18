using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

internal class FamilyViewModel : ItemCheckViewModel {
    public RevitFamily RevitFamily { get; set; }
    public Family Family => RevitFamily?.Family;
    public IEnumerable<RevitParam> OrderedParams => RevitFamily?.OrderedParams ?? [];
}
