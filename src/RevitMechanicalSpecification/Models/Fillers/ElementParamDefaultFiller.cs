using Autodesk.Revit.DB;

using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Models.Fillers {
    public class ElementParamDefaultFiller : ElementParamFiller {
        public ElementParamDefaultFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document) :
            base(toParamName, fromParamName, specConfiguration, document) { }

        public override void SetParamValue(SpecificationElement specificationElement) {
            TargetParam.Set(OriginalParam.AsValueString());
        }
    }
}

