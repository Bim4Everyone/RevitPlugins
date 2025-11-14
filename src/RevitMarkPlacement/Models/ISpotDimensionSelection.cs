using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models;

internal interface ISpotDimensionSelection : ISelection<SpotDimension> {
    Selections Selections { get; }
}
