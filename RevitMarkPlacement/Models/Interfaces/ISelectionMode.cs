using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal interface ISelectionMode {
        IEnumerable<SpotDimension> GetSpotDimentions(Document doc);
    }
}
