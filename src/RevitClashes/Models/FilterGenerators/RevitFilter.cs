using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.FilterGenerators {
    internal class RevitFilter : IFilter {
        public ElementFilter Filter { get; set; }
    }
}
