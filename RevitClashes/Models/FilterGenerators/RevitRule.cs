using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterGenerators {
    internal class RevitRule : IRule {
        public FilterRule FilterRule { get; set; }
    }
}
