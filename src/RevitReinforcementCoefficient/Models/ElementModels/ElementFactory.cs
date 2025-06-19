using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.Models.ElementModels;
internal class ElementFactory {
    private readonly IReportService _reportService;

        public ElementFactory(IReportService reportService) => _reportService = reportService;

    public ICommonElement CreateSpecificElement(Element elem) {

        return elem.InAnyCategory(BuiltInCategory.OST_Rebar) ? new RebarElement(elem, _reportService) : new FormworkElement(elem);
    }
}
