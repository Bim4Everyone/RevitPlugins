using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models.Report {
    internal interface IReportService {
        List<ReportItemSimple> ReportItems { get; set; }
        void AddReportItem(string paramName, ElementId elementId);
    }
}
