using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal class FormworkElement : ICommonElement {
        private readonly IReportService _reportService;

        public FormworkElement(Element element, IReportService reportService) {
            _reportService = reportService;
            RevitElement = element;
        }

        public Element RevitElement { get; set; }

        /// <summary>
        /// Расчет объема одного опалубочного элемента конструкции
        /// </summary>
        public double Calculate() {
            double volumeInInternal = (double) RevitElement.GetParamValue(BuiltInParameter.HOST_VOLUME_COMPUTED);
            return UnitUtilsHelper.ConvertVolumeFromInternalValue(volumeInInternal);
        }
    }
}
