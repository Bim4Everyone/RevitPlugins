using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;

internal class OperationResult {
    public List<OperationResultItem> Items { get; set; } = [];
    public List<ElementId> ElementsToDelete { get; set; } = [];
    public bool Success { get; set; }
}
