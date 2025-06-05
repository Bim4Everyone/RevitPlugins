using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models;
internal class PatternsHelper {
    public PatternsHelper(FillPatternElement fillPatternElement, List<FillPatternElement> fillPatternElements) {
        Pattern = fillPatternElement;
        Patterns = fillPatternElements;
        PatternName = fillPatternElement.Name;
    }

    public string PatternName { get; set; }
    public FillPatternElement Pattern { get; set; }
    public List<FillPatternElement> Patterns { get; set; }
}
