using System;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models;
internal class RoomBorder {

    public RoomBorder(Curve curve) {
        Curve = curve ?? throw new ArgumentNullException(nameof(curve));
        Guid = Guid.NewGuid();
    }

    public Curve Curve { get; }

    public Guid Guid { get; }
}
