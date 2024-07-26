using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models.Classes;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamFiller {

        private readonly DuctElementsCalculator _calculator;

        public ElementParamNameFiller(
            string toParamName,
            string fromParamName, 
            SpecConfiguration specConfiguration,
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = new DuctElementsCalculator(Config, Document);
        }

        private string GetName(Element element) {
            string name = GetTypeOrInstanceParamValue(element);

            if (String.IsNullOrEmpty(name)) 
                { name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                { name += _calculator.GetDuctName(element); }
            }
            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                { name = _calculator.GetDuctFittingName(element); }
            }

            if(element.Category.IsId(BuiltInCategory.OST_PipeFitting)) {
                //Если учет фитингов труб отключен в проекте, не учитываем. Если включен в проекте, но выключен в трубе - не учитываем
                bool isSpecifyPipeFitting = _calculator.IsSpecifyPipeFittingName(element);

                if(!isSpecifyPipeFitting) 
                    { name = "!Не учитывать"; }
            }

            if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                name += _calculator.GetPipeSize(element);
            }

            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) 
                {

                InsulationLiningBase insulation = element as InsulationLiningBase;
                Element pipe = Document.GetElement(insulation.HostElementId);

                if(!(pipe is null)) 
                    { name += " (Для: " + GetName(pipe) + ")"; }
            }

            return name;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedName, GetName(element))); 
        }
    }
}
