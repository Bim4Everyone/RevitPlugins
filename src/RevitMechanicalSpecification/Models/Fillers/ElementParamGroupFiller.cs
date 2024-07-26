using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamGroupFiller : ElementParamFiller {
        public ElementParamGroupFiller(
            string toParamName, 
            string fromParamName, 
            SpecConfiguration specConfiguration, 
            Document document) : 
            base(toParamName, fromParamName, specConfiguration, document) {
        }

        private string GetBaseGroup(Element element) 
            {
            if(element.InAnyCategory(new HashSet<BuiltInCategory>() {
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers})) 
                { return "1.Оборудование"; }

            if(element.Category.IsId(BuiltInCategory.OST_DuctAccessory)) 
                { return"2. Арматура воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctTerminal)) 
                { return"3. Воздухораспределители"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) 
                { return"4. Воздуховоды"; }
            if(element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves)) 
                { return"4. Гибкие воздуховоды"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) 
                { return"5. Фасонные детали воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) 
                { return"6. Материалы изоляции воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeAccessory)) 
                { return"7. Трубопроводная арматура"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) 
                { return"8. Трубопроводы"; }
            if(element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves)) 
                { return"9. Гибкие трубопроводы"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeFitting)) 
                { return"10. Фасонные детали трубопроводов"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) 
                { return"11. Материалы трубопроводной изоляции"; }

            return "Неизвестная категория";
        }

        private string GetGroup(Element element) {
            string baseGroup = GetBaseGroup(element);
            string name = element.GetSharedParamValueOrDefault<string>(Config.TargetNameName);
            string mark = element.GetSharedParamValueOrDefault<string>(Config.TargetNameMark);
            string code = element.GetSharedParamValueOrDefault<string>(Config.TargetNameCode);
            string creator = element.GetSharedParamValueOrDefault<string>(Config.TargetNameCreator);
            return baseGroup + "_" + name + "_" + mark + "_" + code + "_" + creator;
        }

        public override void SetParamValue(Element element) {
            ToParam.Set(element.GetSharedParamValueOrDefault(Config.ForcedGroup, GetGroup(element)));
        }
    }
}
