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
        public string GlobalFunction;
        public string OriginalParamNameName;
        public string OriginalParamNameMark;
        public string OriginalParamNameCode;
        public string OriginalParamNameUnit;
        public string OriginalParamNameCreator;
        public string GlobalSystem;
        public double PipeInsulationStock;
        public double DuctInsulationStock;
        public double DuctAndPipeStock;
        public bool IsSpecifyDuctFittings;
        public bool IsSpecifyPipeFittings;

        public readonly string TargetNameGroup = "ФОП_ВИС_Группирование";
        public readonly string TargetNameFunction = "ФОП_Экономическая функция";
        public readonly string TargetNameSystem = "ФОП_ВИС_Имя системы";
        public readonly string TargetNameName = "ФОП_ВИС_Наименование комбинированное";
        public readonly string TargetNameMark = "ФОП_ВИС_Марка";
        public readonly string TargetNameCode = "ФОП_ВИС_Код изделия";
        public readonly string TargetNameUnit = "ФОП_ВИС_Единица измерения";
        public readonly string TargetNameCreator = "ФОП_ВИС_Завод-изготовитель";
        public readonly string TargetNameNumber = "ФОП_ВИС_Число";

        public readonly string NameAddition = "ФОП_ВИС_Дополнение к имени";
        public readonly string ForcedName = "ФОП_ВИС_Наименование принудительное";
        public readonly string ForcedSystemName = "ФОП_ВИС_Имя системы принудительное";
        public readonly string ForcedGroup = "ФОП_ВИС_Группирование принудительное";
        public readonly string ForcedFunction = "ФОП_ВИС_Экономическая функция";
        

        public readonly string MinDuctThikness = "ФОП_ВИС_Минимальная толщина воздуховода";
        public readonly string MaxDuctThikness = "ФОП_ВИС_Максимальная толщина воздуховода";

        private readonly string _changedNameName = "ФОП_ВИС_Замена параметра_Наименование";
        private readonly string _changedNameMark = "ФОП_ВИС_Замена параметра_Марка";
        private readonly string _changedNameCode = "ФОП_ВИС_Замена параметра_Код изделия";
        private readonly string _changedNameUnit = "ФОП_ВИС_Замена параметра_Единица измерения";
        private readonly string _changedNameCreator = "ФОП_ВИС_Замена параметра_Завод-изготовитель";
        private readonly string _outSystemNameParam = "ФОП_ВИС_Имя внесистемных элементов";

        private readonly string _paramNameIsSpecifyPipeFittings = "ФОП_ВИС_Учитывать фитинги труб";
        public readonly string ParamNameIsSpecifyPipeFittingsFromPype = "ФОП_ВИС_Учитывать фитинги труб по типу трубы";
        private readonly string _paramNameIsSpecifyDuctFittings = "ФОП_ВИС_Учитывать фитинги воздуховодов";

        private readonly string _paramNamePipeInsulationStock = "ФОП_ВИС_Запас изоляции труб";
        private readonly string _paramNameDuctInsulationStock = "ФОП_ВИС_Запас изоляции воздуховодов";
        private readonly string _paramNameDuctPipeStock = "ФОП_ВИС_Запас воздуховодов/труб";
        public readonly string IndividualStock = "ФОП_ВИС_Индивидуальный запас";

        public readonly string IsManiFoldParamName = "ФОП_ВИС_Узел";
        public readonly string IsOutSideOfManifold = "ФОП_ВИС_Исключить из узла";

        public readonly string SingleUnit = "шт.";
        public readonly string KitUnit = "компл.";
        public readonly string MeterUnit = "м.п.";
        public readonly string SquareUnit = "м²";

        public readonly string Dy = "ФОП_ВИС_Ду";
        public readonly string DyWall = "ФОП_ВИС_Ду х Стенка";
        public readonly string DExternalWall = "ФОП_ВИС_Днар х Стенка";

        public readonly string MaskMarkName = "ФОП_ВИС_Маска марки";
        public readonly string MaskNameName = "ФОП_ВИС_Маска наименования";

        public SpecConfiguration(ProjectInfo info) {
            GlobalSystem = info.GetParamValueOrDefault(_outSystemNameParam, "!Нет системы");
            OriginalParamNameName = info.GetParamValueOrDefault(_changedNameName, "ADSK_Наименование");
            OriginalParamNameMark = info.GetParamValueOrDefault(_changedNameMark, "ADSK_Марка");
            OriginalParamNameCode = info.GetParamValueOrDefault(_changedNameCode, "ADSK_Код изделия");
            OriginalParamNameUnit = info.GetParamValueOrDefault(_changedNameUnit, "ADSK_Единица измерения");
            OriginalParamNameCreator = info.GetParamValueOrDefault(_changedNameCreator, "ADSK_Завод-изготовитель");
            GlobalFunction = info.GetParamValueOrDefault(TargetNameFunction, "!Нет функции");
            PipeInsulationStock = info.GetParamValueOrDefault<double>(_paramNamePipeInsulationStock, 0);
            DuctInsulationStock = info.GetParamValueOrDefault<double>(_paramNameDuctInsulationStock, 0);

            DuctAndPipeStock = info.GetParamValueOrDefault<double>(_paramNameDuctPipeStock, 0);

            IsSpecifyDuctFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyDuctFittings) == 1;
            IsSpecifyPipeFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyPipeFittings) == 1;
        }

    }
}
