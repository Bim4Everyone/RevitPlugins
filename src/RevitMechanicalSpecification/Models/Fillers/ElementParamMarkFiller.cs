using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamMarkFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        public ElementParamMarkFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            VisElementsCalculator calculator,
            Document document) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = calculator;
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            TargetParameter.Set(GetMark(specificationElement));
        }

        /// <summary>
        /// Копирует марку из параметра ADSK(или замененного)_Марка или ищет где скопировать 
        /// в случае фитингов воздуховодов
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetMark(SpecificationElement specificationElement) {
            string mark = specificationElement.GetTypeOrInstanceParamStringValue(OriginalParamName);

            if(specificationElement.BuiltInCategory == BuiltInCategory.OST_DuctFitting) {
                mark = _calculator.GetDuctFittingMark(specificationElement.Element);
            }

            return mark;
        }
    }
}
