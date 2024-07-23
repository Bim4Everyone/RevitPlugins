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
        //private readonly Document _document;



        private readonly string _paramNameName;
        private readonly string _paramNameMark;
        private readonly string _paramNameCode;
        private readonly string _paramNameUnit;
        private readonly string _paramNameCreator;
        private readonly double _insulationStock;
        private readonly double _ductAndPipeStock;
        private readonly bool _isSpecifyDuctFittings;
        private readonly bool _isSpecifyPipeFittings;




        public string ParamNameName { get { return _paramNameName; } }
        public string ParamNameMark { get { return _paramNameMark; } }
        public string ParamNameCode { get { return _paramNameCode; } }
        public string ParamNameUnit {  get { return _paramNameUnit; } }
        public string ParamNameCreator { get { return _paramNameCreator; } }
        public double InsulationStock { get { return _insulationStock; } }
        public double DuctAndPipeStock { get { return _ductAndPipeStock; } }
        public bool IsSpecifyDuctFittings { get { return _isSpecifyDuctFittings; } }
        public bool IsSpecifyPipeFittings { get { return _isSpecifyPipeFittings; } }

        public readonly string TargetNameGroup = "ФОП_ВИС_Группирование";
        public readonly string TargetNameFunction = "ФОП_Экономическая функция";
        public readonly string TargetNameVisFunction = "ФОП_ВИС_Экономическая функция";
        public readonly string TargetNameName = "ФОП_ВИС_Наименование комбинированное";
        public readonly string TargetNameMark = "ФОП_ВИС_Марка";
        public readonly string TargetNameCode = "ФОП_ВИС_Код изделия";
        public readonly string TargetNameUnit = "ФОП_ВИС_Единица измерения";
        public readonly string TargetNameCreator = "ФОП_ВИС_Завод-изготовитель";
        public readonly string TargetNameNumber = "ФОП_ВИС_Число";

        private readonly string _changedNameName = "ФОП_ВИС_Замена параметра_Наименование";
        private readonly string _changedNameMark = "ФОП_ВИС_Замена параметра_Марка";
        private readonly string _changedNameCode = "ФОП_ВИС_Замена параметра_Код изделия";
        private readonly string _changedNameUnit = "ФОП_ВИС_Замена параметра_Единица измерения";
        private readonly string _changedNameCreator = "ФОП_ВИС_Замена параметра_Завод-изготовитель";

        private readonly string _paramNameIsSpecifyPipeFittings = "ФОП_ВИС_Учитывать фитинги труб";
        private readonly string _paramNameIsSpecifyDuctFittings = "ФОП_ВИС_Учитывать фитинги воздуховодов";

        private readonly string _paramNameInsulationStock = "ФОП_ВИС_Запас изоляции";
        private readonly string _paramNameDuctPipeStock = "ФОП_ВИС_Запас воздуховодов/труб";

        public readonly string SingleUnit = "шт.";
        public readonly string KitUnit = "компл.";
        public readonly string MeterUnit = "м.п.";
        public readonly string SquareUnit = "м²";


        public SpecConfiguration(ProjectInfo info) {
            _paramNameName = info.GetParamValueOrDefault(_changedNameName, "ADSK_Наименование");
            _paramNameMark = info.GetParamValueOrDefault(_changedNameMark, "ADSK_Марка");
            _paramNameCode = info.GetParamValueOrDefault(_changedNameCode, "ADSK_Код изделия");
            _paramNameUnit = info.GetParamValueOrDefault(_changedNameUnit, "ADSK_Единица измерения");
            _paramNameCreator = info.GetParamValueOrDefault(_changedNameCreator, "ADSK_Завод-изготовитель");
            _insulationStock = info.GetParamValueOrDefault<double>(_paramNameInsulationStock, 0);
            _ductAndPipeStock = info.GetParamValueOrDefault<double>(_paramNameDuctPipeStock, 0);

            _isSpecifyPipeFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyPipeFittings) == 1;
            _isSpecifyDuctFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyDuctFittings) == 1;
        }

    }
}
