using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal class ElementFactory {
        private readonly IReportService _reportService;

        public ElementFactory(IReportService reportService) => _reportService = reportService;

        public ICommonElement CreateSpecificElement(Element elem) {

            if(elem.InAnyCategory(BuiltInCategory.OST_Rebar)) {
                return new RebarElement(elem, _reportService);
            } else {
                return new FormworkElement(elem);
            }
        }
    }
}
