using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {
    internal class SpecConfiguration {
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

        private readonly string _targetNameName = "ФОП_ВИС_Наименование комбинированное";
        private readonly string _targetNameMark = "ФОП_ВИС_Марка";
        private readonly string _targetNameCode = "ФОП_ВИС_Код изделия";
        private readonly string _targetNameUnit = "ФОП_ВИС_Единица измерения";
        private readonly string _targetNameCreator = "ФОП_ВИС_Завод изготовитель";

        private readonly string _changedNameName = "ФОП_ВИС_Замена параметра_Наименование";
        private readonly string _changedNameMark = "ФОП_ВИС_Замена параметра_Марка";
        private readonly string _changedNameCode = "ФОП_ВИС_Замена параметра_Код изделия";
        private readonly string _changedNameUnit = "ФОП_ВИС_Замена параметра_Единица измерения";
        private readonly string _changedNameCreator = "ФОП_ВИС_Замена параметра_Завод-изготовитель";

        private readonly string _paramNameIsSpecifyPipeFittings = "ФОП_ВИС_Учитывать фитинги труб";
        private readonly string _paramNameIsSpecifyDuctFittings = "ФОП_ВИС_Учитывать фитинги воздуховодов"

        private readonly string _paramNameInsulationStock = "ФОП_ВИС_Запас изоляции";
        private readonly string _paramNameDuctPipeStock = "ФОП_ВИС_Запас воздуховодов/труб";


        public SpecConfiguration(ProjectInfo info) {

            ParamNameName = info.GetParamValueOrDefault(_changedNameName, "ADSK_Наименование");
            ParamNameMark = info.GetParamValueOrDefault(_changedNameMark, "ADSK_Марка");
            ParamNameCode = info.GetParamValueOrDefault(_changedNameCode, "ADSK_Код изделия");
            ParamNameUnit = info.GetParamValueOrDefault(_changedNameUnit, "ADSK_Единица измерения");
            ParamNameCreator = info.GetParamValueOrDefault(_changedNameCreator, "ADSK_Завод-изготовитель");
            InsulationStock = info.GetParamValueOrDefault<double>(_paramNameInsulationStock, 0);
            DuctAndPipeStock = info.GetParamValueOrDefault<double>(_paramNameDuctPipeStock, 0);

            IsSpecifyPipeFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyPipeFittings) == 1;
            IsSpecifyDuctFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyDuctFittings) == 1;
        }

    }
}
