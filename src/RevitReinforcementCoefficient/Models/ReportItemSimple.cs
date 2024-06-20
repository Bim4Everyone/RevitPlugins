using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models {
    internal class ReportItemSimple {
        public ReportItemSimple(string paramName, ElementId elementId) {
            ParamName = paramName;
            ElementId = elementId;
        }

        public string ParamName { get; set; }

        public ElementId ElementId { get; set; }
    }
}
