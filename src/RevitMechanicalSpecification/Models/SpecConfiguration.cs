using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {
    public class SpecConfiguration {
        public readonly string GlobalFunction;
        public readonly string OriginalParamNameName;
        public readonly string OriginalParamNameMark;
        public readonly string OriginalParamNameCode;
        public readonly string OriginalParamNameUnit;
        public readonly string OriginalParamNameCreator;
        public readonly string GlobalSystem;
        public readonly double PipeInsulationStock;
        public readonly double DuctInsulationStock;
        public readonly double DuctAndPipeStock;
        public readonly bool IsSpecifyDuctFittings;
        public readonly bool IsSpecifyPipeFittings;

        public readonly string TargetNameGroup = SharedParamsConfig.Instance.VISGrouping.Name; //ФОП_ВИС_Группирование
        public readonly string TargetNameFunction = SharedParamsConfig.Instance.EconomicFunction.Name; //"ФОП_Экономическая функция";
        public readonly string TargetNameSystem = SharedParamsConfig.Instance.VISSystemName.Name;//"ФОП_ВИС_Имя системы";
        public readonly string TargetNameName = SharedParamsConfig.Instance.VISCombinedName.Name;//"ФОП_ВИС_Наименование комбинированное";
        public readonly string TargetNameMark = SharedParamsConfig.Instance.VISMarkNumber.Name;//"ФОП_ВИС_Марка";
        public readonly string TargetNameCode = SharedParamsConfig.Instance.VISItemCode.Name;//"ФОП_ВИС_Код изделия";
        public readonly string TargetNameUnit = SharedParamsConfig.Instance.VISUnit.Name; //"ФОП_ВИС_Единица измерения";
        public readonly string TargetNameCreator = SharedParamsConfig.Instance.VISManufacturer.Name; //"ФОП_ВИС_Завод-изготовитель";
        public readonly string TargetNameNumber = SharedParamsConfig.Instance.VISSpecNumbers.Name; //"ФОП_ВИС_Число";

        public readonly string NameAddition = SharedParamsConfig.Instance.VISNameAddition.Name; //"ФОП_ВИС_Дополнение к имени";
        public readonly string ForcedName = SharedParamsConfig.Instance.VISNameForced.Name; //"ФОП_ВИС_Наименование принудительное";
        public readonly string ForcedSystemName = SharedParamsConfig.Instance.VISSystemNameForced.Name; //"ФОП_ВИС_Имя системы принудительное";
        public readonly string ForcedGroup = SharedParamsConfig.Instance.VISGroupingForced.Name; //"ФОП_ВИС_Группирование принудительное";
        public readonly string ForcedFunction = SharedParamsConfig.Instance.VISEconomicFunction.Name;//"ФОП_ВИС_Экономическая функция" - замена принудительной функции;

        
        public readonly string SystemEF = SharedParamsConfig.Instance.VISHvacSystemFunction.Name;//"ФОП_ВИС_ЭФ для системы"
        public readonly string SystemShortName = SharedParamsConfig.Instance.VISSystemShortName.Name;//"ФОП_ВИС_Сокращение для системы"


        public readonly string MinDuctThikness = SharedParamsConfig.Instance.VISMinDuctThickness.Name; //"ФОП_ВИС_Минимальная толщина воздуховода";
        public readonly string MaxDuctThikness = SharedParamsConfig.Instance.VISMaxDuctThickness.Name; //"ФОП_ВИС_Максимальная толщина воздуховода";

        private readonly string _changedNameName = SharedParamsConfig.Instance.VISParamReplacementName.Name; //"ФОП_ВИС_Замена параметра_Наименование";
        private readonly string _changedNameMark = SharedParamsConfig.Instance.VISParamReplacementMarkNumber.Name; //"ФОП_ВИС_Замена параметра_Марка";
        private readonly string _changedNameCode = SharedParamsConfig.Instance.VISParamReplacementItemCode.Name; //"ФОП_ВИС_Замена параметра_Код изделия";
        private readonly string _changedNameUnit = SharedParamsConfig.Instance.VISParamReplacementUnit.Name; //"ФОП_ВИС_Замена параметра_Единица измерения";
        private readonly string _changedNameCreator = SharedParamsConfig.Instance.VISParamReplacementManufacturer.Name; //"ФОП_ВИС_Замена параметра_Завод-изготовитель";
        private readonly string _outSystemNameParam = SharedParamsConfig.Instance.VISOutSystemName.Name; //"ФОП_ВИС_Имя внесистемных элементов";

        private readonly string _paramNameIsSpecifyPipeFittings = SharedParamsConfig.Instance.VISConsiderPipeFittings.Name; //"ФОП_ВИС_Учитывать фитинги труб";
        public readonly string ParamNameIsSpecifyPipeFittingsFromPype = SharedParamsConfig.Instance.VISConsiderPipeFittingsByType.Name; //"ФОП_ВИС_Учитывать фитинги труб по типу трубы";
        private readonly string _paramNameIsSpecifyDuctFittings = SharedParamsConfig.Instance.VISConsiderDuctFittings.Name; //"ФОП_ВИС_Учитывать фитинги воздуховодов";

        public readonly string ParamNamePipeInsulationStock = SharedParamsConfig.Instance.VISPipeInsulationReserve.Name; //"ФОП_ВИС_Запас изоляции труб";
        public readonly string ParamNameDuctInsulationStock = SharedParamsConfig.Instance.VISDuctInsulationReserve.Name; //"ФОП_ВИС_Запас изоляции воздуховодов";
        public readonly string ParamNameDuctPipeStock = SharedParamsConfig.Instance.VISPipeDuctReserve.Name; //"ФОП_ВИС_Запас воздуховодов/труб";
        public readonly string IndividualStock = SharedParamsConfig.Instance.VISIndividualStock.Name; //"ФОП_ВИС_Индивидуальный запас";

        public readonly string IsManiFoldParamName = SharedParamsConfig.Instance.VISJunction.Name; //"ФОП_ВИС_Узел";
        public readonly string IsOutSideOfManifold = SharedParamsConfig.Instance.VISExcludeFromJunction.Name; //"ФОП_ВИС_Исключить из узла";

        public readonly string SingleUnit = "шт.";
        public readonly string KitUnit = "компл.";
        public readonly string MeterUnit = "м.п.";
        public readonly string SquareUnit = "м²";

        public readonly string Dy = SharedParamsConfig.Instance.VISDiameterNominal.Name; //"ФОП_ВИС_Ду";
        public readonly string DyWall = SharedParamsConfig.Instance.VISDiameterNominalXThikness.Name; //"ФОП_ВИС_Ду х Стенка";
        public readonly string DExternalWall = SharedParamsConfig.Instance.VISDiameterExternalXThikness.Name; //"ФОП_ВИС_Днар х Стенка";

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
            PipeInsulationStock = 1 + (info.GetParamValueOrDefault<double>(ParamNamePipeInsulationStock, 0))/100;
            DuctInsulationStock = 1 + (info.GetParamValueOrDefault<double>(ParamNameDuctInsulationStock, 0))/100;

            DuctAndPipeStock = 1 + (info.GetParamValueOrDefault<double>(ParamNameDuctPipeStock, 0))/100;

            IsSpecifyDuctFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyDuctFittings) == 1;
            IsSpecifyPipeFittings = info.GetSharedParamValueOrDefault<int>(_paramNameIsSpecifyPipeFittings) == 1;
        }
    }
}
