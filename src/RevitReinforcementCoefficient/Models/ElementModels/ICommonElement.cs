using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal interface ICommonElement {

        Element RevitElement { get; set; }
        double Calculate();
    }
}
