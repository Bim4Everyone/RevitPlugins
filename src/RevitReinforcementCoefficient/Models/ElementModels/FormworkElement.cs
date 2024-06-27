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
            var volumeValue = RevitElement.GetParamValue(BuiltInParameter.HOST_VOLUME_COMPUTED);
            if(volumeValue is null) {
                return 0;
            } else {
                return UnitUtilsHelper.ConvertVolumeFromInternalValue((double) volumeValue);
            }
        }
    }
}
