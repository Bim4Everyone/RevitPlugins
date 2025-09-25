using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models.Interfaces;

internal interface ILintelsProvider {
    ICollection<FamilyInstance> GetLintels();
}
