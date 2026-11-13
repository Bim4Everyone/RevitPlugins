using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Dimensions.DimensionReferences.ReferenceCollector;
internal interface IReferenceCollectorContext {
    List<Reference> ElementReferences { get; set; }
    XYZ Direction { get; set; }
}
