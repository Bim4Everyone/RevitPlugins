using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal class FormworkElement : ICommonElement {
        public FormworkElement(Element element) {
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
