using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class NameAndGroupFactory {
        private readonly SpecConfiguration _config;
        private readonly Document _document;
        private readonly VisElementsCalculator _calculator;

        public NameAndGroupFactory(SpecConfiguration configuration, Document document, VisElementsCalculator calculator) {
            _document = document;
            _config = configuration;
            _calculator = calculator;
        }

        //Ниже операции с именами
        //Базовое имя без учета узла
        private string GetBaseName(Element element) {
            Element elemType = element.GetElementType();
            string name = GetTypeOrInstanceParamValue(element, elemType, _config.OriginalParamNameName);

            if(string.IsNullOrEmpty(name)) { name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                { name += _calculator.GetDuctName(element); }
            }
            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {
                { name = _calculator.GetDuctFittingName(element); }
            }
            if(element.Category.IsId(BuiltInCategory.OST_PipeFitting)) {
                //Если учет фитингов труб отключен в проекте, не учитываем. Если включен в проекте, но выключен в трубе - не учитываем
                bool isSpecifyPipeFitting = _calculator.IsSpecifyPipeFittingName(element);

                if(!isSpecifyPipeFitting) { name = "!Не учитывать"; }
            }
            if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                name += _calculator.GetPipeSize(element);
            }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) {

                var insulation = element as InsulationLiningBase;
                Element pipe = _document.GetElement(insulation.HostElementId);

                if(!(pipe is null)) { name += " (Для: " + GetName(pipe) + ")"; }
            }
            return name;
        }
        //Базовое имя + модификация если узел
        public string GetName(Element element) {
            string name = GetBaseName(element);

            if(element is FamilyInstance instance) {
                if(!IsOutSideOfManifold(element)) {
                    FamilyInstance familyInstance = instance;
                    if(!(familyInstance.SuperComponent is null)) {
                        familyInstance = GetSuperComponentIfExist(familyInstance);

                        if(IsManifold(familyInstance)) {
                            string num = GetNumeration(familyInstance, element);
                            if(!string.IsNullOrEmpty(num)) { name = num + name; }
                        }
                    }
                }
            }
            return name;
        }

        //Ниже операции с группами
        //Базовая группа-основа сортировки
        private string GetBaseGroup(Element element) {
            if(element.InAnyCategory(new HashSet<BuiltInCategory>() {
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers})) { return "1. Оборудование"; }

            if(element.Category.IsId(BuiltInCategory.OST_DuctAccessory)) { return "2. Арматура воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctTerminal)) { return "3. Воздухораспределители"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) { return "4. Воздуховоды"; }
            if(element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves)) { return "4. Гибкие воздуховоды"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) { return "5. Фасонные детали воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) { return "6. Материалы изоляции воздуховодов"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeAccessory)) { return "7. Трубопроводная арматура"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) { return "8. Трубопроводы"; }
            if(element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves)) { return "9. Гибкие трубопроводы"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeFitting)) { return "10. Фасонные детали трубопроводов"; }
            if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) { return "11. Материалы трубопроводной изоляции"; }

            return "Неизвестная категория";
        }
        //Детализированная - имя + марка + код + завод
        private string GetDetailedGroup(Element element) {
            Element elemType = element.GetElementType();

            string name = GetBaseName(element);
            string mark = GetTypeOrInstanceParamValue(element, elemType, _config.OriginalParamNameMark);
            string code = GetTypeOrInstanceParamValue(element, elemType, _config.OriginalParamNameCode);
            string creator = GetTypeOrInstanceParamValue(element, elemType, _config.OriginalParamNameCreator);
            return "_" + name + "_" + mark + "_" + code + "_" + creator;
        }
        //Базовая + детализированная + модификация если узел
        public string GetGroup(Element element) {
            string detailedGroup = GetDetailedGroup(element);

            if(element is FamilyInstance instance && !IsOutSideOfManifold(element)) {
                if(instance.SuperComponent != null) {
                    FamilyInstance familyInstance = GetSuperComponentIfExist(instance);

                    if(IsManifold(familyInstance)) {
                        return
                            $"{GetBaseGroup(familyInstance)}" +
                            $"_Узел_" +
                            $"{familyInstance.GetParamValue(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)}{detailedGroup}";
                    }
                }
            }
            return $"{GetBaseGroup(element)}{detailedGroup}";
        }
        //Получает нумерацию вложения в коллектор
        public string GetNumeration(FamilyInstance family, Element currentObj) {
            int num = 1;
            string lastGroup = string.Empty;

            var elementsOfManifold = GetSub(family).OrderBy(e => GetGroup(e)).ToList();

            foreach(Element element in elementsOfManifold) {
                string name = GetBaseName(element);

                if(!string.IsNullOrEmpty(name) && name != "!Не учитывать") {
                    string currentGroup = GetGroup(currentObj);
                    string elemGroup = GetGroup(element);
                    if(lastGroup != elemGroup) {
                        lastGroup = elemGroup;
                        if(currentGroup == elemGroup) {
                            return "‎    " + num.ToString() + ". ";
                        }
                        num++;
                    }
                }
            }
            return null;
        }


        //Ниже операции с узлами
        //если есть суперкомпонент - возвращает его. Иначе возвращает исходник
        public FamilyInstance GetSuperComponentIfExist(FamilyInstance instance) {
            if(!(instance.SuperComponent is null)) {
                instance = (FamilyInstance) instance.SuperComponent;
                instance = GetSuperComponentIfExist(instance);
            }
            return instance;
        }

        //возвращает субкомпоненты и субкомпоненты субкомпонентов
        private List<Element> GetSub(FamilyInstance element) {
            var subs = new List<Element>();

            foreach(ElementId elementId in element.GetSubComponentIds()) {
                Element subElement = _document.GetElement(elementId);
                subs.Add(subElement);

                var subInst = subElement as FamilyInstance;
                if(subInst.GetSubComponentIds().Count > 0) { subs.AddRange(GetSub(subInst)); }
            }
            return subs;
        }

        //возвращает значение параметра по типу или экземпляру, если существует, иначе null
        private string GetTypeOrInstanceParamValue(Element element, Element elemType, string paraName) {
            if(element.IsExistsParam(paraName)) { return element.GetSharedParamValueOrDefault<string>(paraName); }
            if(elemType.IsExistsParam(paraName)) { return elemType.GetSharedParamValueOrDefault<string>(paraName); }
            return null;
        }


        private bool IsManifold(Element element) {
            Element elemType = element.GetElementType();

            if(!(elemType.GetSharedParamValueOrDefault<int>(_config.IsManiFoldParamName) == 1)) { return false; }

            return true;
        }

        private bool IsOutSideOfManifold(Element element) {
            Element elemType = element.GetElementType();
            if(elemType.GetSharedParamValueOrDefault<int>(_config.IsOutSideOfManifold) == 1) {
                return true;
            }
            return false;

        }

    }
}
