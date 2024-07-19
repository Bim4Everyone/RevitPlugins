using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {
    internal class SpecConfiguration 
        {
        private readonly Document _document;
        public string ParamNameName;
        public string ParamNameMark;
        public string ParamNameCode;
        public string ParamNameUnit;
        public string ParamNameCreator;
        public double InsulationStock;
        public double DuctAndPipeStock;
        public bool IsSpecifyDuctFittings;
        public bool IsSpecifyPipeFittings;


        public SpecConfiguration(Document document) 
            {

            _document = document;
            ParamNameName = _document.ProjectInformation.GetParamValueOrDefault("ФОП_ВИС_Замена параметра_Наименование", "ADSK_Наименование");
            ParamNameMark = _document.ProjectInformation.GetParamValueOrDefault("ФОП_ВИС_Замена параметра_Марка", "ADSK_Марка");
            ParamNameCode = _document.ProjectInformation.GetParamValueOrDefault("ФОП_ВИС_Замена параметра_Код изделия", "ADSK_Код изделия");
            ParamNameUnit = _document.ProjectInformation.GetParamValueOrDefault("ФОП_ВИС_Замена параметра_Единица измерения", "ADSK_Единица измерения");
            ParamNameCreator = _document.ProjectInformation.GetParamValueOrDefault("ФОП_ВИС_Замена параметра_Завод-изготовитель", "ADSK_Завод-изготовитель");
            InsulationStock = _document.ProjectInformation.GetParamValueOrDefault<double>("ФОП_ВИС_Запас изоляции", 0);
            DuctAndPipeStock = _document.ProjectInformation.GetParamValueOrDefault<double>("ФОП_ВИС_Запас воздуховодов/труб", 0);

            IsSpecifyPipeFittings = GetYesNoParam(_document.ProjectInformation.GetParam("ФОП_ВИС_Учитывать фитинги труб"));
            IsSpecifyDuctFittings = GetYesNoParam(_document.ProjectInformation.GetParam("ФОП_ВИС_Учитывать фитинги воздуховодов"));
            }


        private bool GetYesNoParam(Parameter parameter) 
            {
            int value = parameter.AsInteger();
            if (value == 1) 
                {
                return true;
                }
            return false;
            }

    }
}
