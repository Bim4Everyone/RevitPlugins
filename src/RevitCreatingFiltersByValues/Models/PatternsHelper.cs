using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models;
internal class PatternsHelper {
    public PatternsHelper(FillPatternElement fillPatternElement, List<FillPatternElement> fillPatternElements) {
        Pattern = fillPatternElement;
        PatternsInPj = fillPatternElements;
        PatternName = fillPatternElement.Name;
    }

    public string PatternName { get; set; }
    public FillPatternElement Pattern { get; set; }
    public List<FillPatternElement> PatternsInPj { get; set; }
}
