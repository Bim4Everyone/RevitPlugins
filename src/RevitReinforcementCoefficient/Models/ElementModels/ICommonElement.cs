using Autodesk.Revit.DB;

using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal interface ICommonElement {

        Element RevitElement { get; set; }
        double Calculate(ReportVM report);
    }
}
