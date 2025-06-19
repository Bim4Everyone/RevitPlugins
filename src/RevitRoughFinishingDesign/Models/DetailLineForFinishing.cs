using System;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models;
internal class DetailLineForFinishing {
    public DetailLineForFinishing(Line line) {
        LineForFinishing = line ?? throw new ArgumentNullException(nameof(line));
        Guid = Guid.NewGuid();
        CalculatedOffset = Offset * LayerNumber - DistanceFromBorder;
    }

    public Line LineForFinishing { get; set; }
    public int LayerNumber { get; set; }
    public double Offset { get; set; }
    public double DistanceFromBorder { get; set; }
    public double CalculatedOffset { get; }
    public ElementId LineStyleId { get; set; }
    public Guid Guid { get; }
}
