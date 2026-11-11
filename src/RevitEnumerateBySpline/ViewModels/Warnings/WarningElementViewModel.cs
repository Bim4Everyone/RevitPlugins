using Autodesk.Revit.DB;

using RevitEnumerateBySpline.Models;

namespace RevitEnumerateBySpline.ViewModels.Warnings;

internal sealed class WarningElementViewModel(SpatialModel spatialModel) {
    public ElementId? ElementId { get; } = spatialModel.SpatialElement?.Id;
    public string Name { get; } = spatialModel.SpatialElement?.Name ?? string.Empty;
    public string LevelName { get; } = spatialModel.LevelName ?? string.Empty;
}
