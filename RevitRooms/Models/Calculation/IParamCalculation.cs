using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitRooms.ViewModels;

namespace RevitRooms.Models.Calculation {
    internal interface IParamCalculation {
        Phase Phase { get; set; }
        RevitParam RevitParam { get; set; }
        bool SetParamValue(SpatialElementViewModel spatialElement);
        void CalculateParam(SpatialElementViewModel spatialElement);
        
        double GetDifferences();
        double GetPercentChange();
    }
}
