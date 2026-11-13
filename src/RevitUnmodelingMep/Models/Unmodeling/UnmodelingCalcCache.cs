using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitUnmodelingMep.Models.Entities;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class UnmodelingCalcCache {
    public Dictionary<ElementId, List<Element>> ElementsByTypeId { get; } = new();
    public Dictionary<ElementId, CalculationElementBase> CalcElementsById { get; } = new();
    public Dictionary<ElementId, (string Block, string Section, string Floor, double Currency)> SmrParamsById { get; } = new();
    public Dictionary<ElementId, (string System, string EconomicFunction)> SystemInfoById { get; } = new();
}
