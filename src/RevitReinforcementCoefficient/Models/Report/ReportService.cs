using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models.Report {
    internal class ReportService : IReportService {
        public ReportService() { }

        public List<ReportItemSimple> ReportItems { get; set; } = new List<ReportItemSimple>();

        public void AddReportItem(string paramName, ElementId elementId) {
            ReportItems.Add(new ReportItemSimple(paramName, elementId));
        }
    }
}
