using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models.Report {
    internal class ReportService : IReportService {
        public ReportService() { }

        public List<ReportItem> ReportItems { get; set; } = new List<ReportItem>();

        public void AddReportItem(string paramName, ElementId elementId) {
            ReportItem error = ReportItems.FirstOrDefault(e => e.ErrorName.Contains($"\"{paramName}\""));
            if(error is null) {
                ReportItems.Add(new ReportItem(paramName, elementId));
            } else {
                error.AddId(elementId);
            }
        }

        public void ClearReportItems() => ReportItems.Clear();
    }
}
