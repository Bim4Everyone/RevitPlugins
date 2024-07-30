using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {
    public class SpecConfiguration {

        //константы сделать константами
        public string OriginalParamNameName;
        public string OriginalParamNameMark;
        public string OriginalParamNameCode;
        public string OriginalParamNameUnit;
        public string OriginalParamNameCreator;
        public double InsulationStock;
        public double DuctAndPipeStock;
        public bool IsSpecifyDuctFittings;
        public bool IsSpecifyPipeFittings;

        public const string TargetNameGroup = "ФОП_ВИС_Группирование";
        public const string TargetNameFunction = "ФОП_Экономическая функция";
        public const string TargetNameSystem = "ФОП_ВИС_Имя системы";
        public const string TargetNameVisFunction = "ФОП_ВИС_Экономическая функция";
        public const string TargetNameName = "ФОП_ВИС_Наименование комбинированное";
        public const string TargetNameMark = "ФОП_ВИС_Марка";
        public const string TargetNameCode = "ФОП_ВИС_Код изделия";
        public const string TargetNameUnit = "ФОП_ВИС_Единица измерения";
        public const string TargetNameCreator = "ФОП_ВИС_Завод-изготовитель";
        public const string TargetNameNumber = "ФОП_ВИС_Число";

        public const string ForcedName = "ФОП_ВИС_Наименование принудительное";
        public const string ForcedSystemName = "ФОП_ВИС_Имя системы принудительное";
        public const string ForcedGroup = "ФОП_ВИС_Группирование принудительное";
        public const string ForcedFunction = "ФОП_ВИС_Функция принудительная";

        public const string MinDuctThikness = "ФОП_ВИС_Минимальная толщина воздуховода";
        public const string MaxDuctThikness = "ФОП_ВИС_Максимальная толщина воздуховода";


        private const string _changedNameName = "ФОП_ВИС_Замена параметра_Наименование";
        private const string _changedNameMark = "ФОП_ВИС_Замена параметра_Марка";
        private const string _changedNameCode = "ФОП_ВИС_Замена параметра_Код изделия";
        private const string _changedNameUnit = "ФОП_ВИС_Замена параметра_Единица измерения";
        private const string _changedNameCreator = "ФОП_ВИС_Замена параметра_Завод-изготовитель";

        private const string _paramNameIsSpecifyPipeFittings = "ФОП_ВИС_Учитывать фитинги труб";
        public const string ParamNameIsSpecifyPipeFittingsFromPype = "ФОП_ВИС_Учитывать фитинги труб по типу трубы";
        private const string _paramNameIsSpecifyDuctFittings = "ФОП_ВИС_Учитывать фитинги воздуховодов";


        private const string _paramNameInsulationStock = "ФОП_ВИС_Запас изоляции";
        private const string _paramNameDuctPipeStock = "ФОП_ВИС_Запас воздуховодов/труб";
        public const string IndividualStock = "ФОП_ВИС_Индивидуальный запас";


        public const string IsManiFoldParamName = "ФОП_ВИС_Узел";
        public const string IsOutSideOfManifold = "ФОП_ВИС_Исключить из узла";

        public const string SingleUnit = "шт.";
        public const string KitUnit = "компл.";
        public const string MeterUnit = "м.п.";
        public const string SquareUnit = "м²";


        public SpecConfiguration(ProjectInfo info) {
            OriginalParamNameName = info.GetParamValueOrDefault(_changedNameName, "ADSK_Наименование");
            OriginalParamNameMark = info.GetParamValueOrDefault(_changedNameMark, "ADSK_Марка");
            OriginalParamNameCode = info.GetParamValueOrDefault(_changedNameCode, "ADSK_Код изделия");
            OriginalParamNameUnit = info.GetParamValueOrDefault(_changedNameUnit, "ADSK_Единица измерения");
            OriginalParamNameCreator = info.GetParamValueOrDefault(_changedNameCreator, "ADSK_Завод-изготовитель");
            InsulationStock = info.GetParamValueOrDefault<double>(_paramNameInsulationStock, 0);
            DuctAndPipeStock = info.GetParamValueOrDefault<double>(_paramNameDuctPipeStock, 0);

            IsSpecifyDuctFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyPipeFittings) == 1;
            IsSpecifyPipeFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyDuctFittings) == 1;
        }

    }
}
