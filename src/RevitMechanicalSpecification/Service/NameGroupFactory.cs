using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Models;

namespace RevitMechanicalSpecification.Service {
    internal class NameGroupFactory {
        private readonly SpecConfiguration _config;
        private readonly Document _document;
        private readonly VisElementsCalculator _calculator;
        public NameGroupFactory(
            SpecConfiguration configuration,
            Document document,
            VisElementsCalculator calculator
            ) {
            _document = document;
            _config = configuration;
            _calculator = calculator;          
        }

        /// <summary>
        /// Базовая + детализированная группа
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetGroup(Element element) {
            string detailedGroup = GetDetailedGroup(element);
            return $"{GetBaseGroup(element)}{detailedGroup}";
        }

        /// <summary>
        /// Возвращает субкомпоненты и субкомпоненты субкомпонентов
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public List<Element> GetSub(FamilyInstance element) {
            var subs = new List<Element>();

            foreach(ElementId elementId in element.GetSubComponentIds()) {
                Element subElement = _document.GetElement(elementId);
                subs.Add(subElement);

                var subInst = subElement as FamilyInstance;
                if(subInst.GetSubComponentIds().Count > 0) {
                    subs.AddRange(GetSub(subInst));
                }
            }
            return subs;
        }

        /// <summary>
        /// Получаем ФОП_ВИС_Группирование для узла
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetManifoldGroup(FamilyInstance familyInstance, Element element) {
            return
                    $"{GetBaseGroup(familyInstance)}" +
                    $"{GetDetailedGroup(familyInstance)}" +
                    $"{familyInstance.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()}" +
                    $"_Узел_" +
                    $"_{GetDetailedGroup(element)}";
        }

        /// <summary>
        /// Проверяем нужно ли увеличивать цифру в наименовании вложений узла, сравнивая группирования этого и прошлого
        /// по индексу элементов
        /// </summary>
        /// <param name="manifoldElements"></param>
        /// <param name="index"></param>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public bool IsIncreaseCount(List<Element> manifoldElements, int index, Element element, Element elemType) {
            string name = GetName(element, elemType);

            if(string.IsNullOrEmpty(name) || name == "!Не учитывать" || index == 0) {
                return false;
            }

            return GetGroup(element) != GetGroup(manifoldElements[index - 1]);
        }

        /// <summary>
        /// Проверяем значение галочки "ФОП_ВИС_Узел"
        /// </summary>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public bool IsManifold(Element elemType) {
            return elemType.GetSharedParamValueOrDefault<int>(_config.IsManiFoldParamName) == 1;
        }

        /// <summary>
        /// Проверяем значение галочки "ФОП_ВИС_Исключить из узла"
        /// </summary>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public bool IsOutSideOfManifold(Element elemType) {
            return elemType.GetSharedParamValueOrDefault<int>(_config.IsOutSideOfManifold) == 1;
        }

        /// <summary>
        /// Базовое наименование
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        public string GetName(Element element, Element elemType) {
            string name = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameName);
            string nameAddon = element.GetTypeOrInstanceParamStringValue(elemType, _config.NameAddition);

            if(string.IsNullOrEmpty(name)) {
                name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ";
            }
            Category category = element.Category;
            switch(category.GetBuiltInCategory()) {
                case BuiltInCategory.OST_DuctCurves:
                    return $"{name} {_calculator.GetDuctName(element, elemType)} {nameAddon}";
                case BuiltInCategory.OST_DuctFitting:
                    return $"{_calculator.GetDuctFittingName(element)} {nameAddon}";
                case BuiltInCategory.OST_PipeFitting:
                    //Если учет фитингов труб отключен в проекте, не учитываем. Если включен в проекте, но выключен в трубе - не учитываем
                    return _calculator.IsSpecifyPipeFittingName(element) ? 
                        $"{name} {nameAddon}" : "!Не учитывать";
                case BuiltInCategory.OST_PipeCurves:
                    return $"{name} {_calculator.GetPipeSize(element, elemType)} {nameAddon}";
                case BuiltInCategory.OST_PipeInsulations:
                    InsulationLiningBase insulation = element as InsulationLiningBase;
                    Element pipe = _document.GetElement(insulation.HostElementId);
                    return pipe != null ? 
                        $"{name} (Для: {GetName(pipe, pipe.GetElementType())}) {nameAddon}" : "!Не учитывать";
                case BuiltInCategory.OST_DuctAccessory:
                    return $"{name} {nameAddon}";
            }

            return $"{name} {nameAddon}";
        }

        /// <summary>
        /// Получаем базовое группирование элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetBaseGroup(Element element) {
            Category category = element.Category;
            switch(category.GetBuiltInCategory()) {
                case BuiltInCategory.OST_MechanicalEquipment:
                    return "1. Оборудование";
                case BuiltInCategory.OST_PlumbingFixtures:
                    return "1. Оборудование";
                case BuiltInCategory.OST_Sprinklers:
                    return "1. Оборудование";
                case BuiltInCategory.OST_DuctAccessory:
                    return "2. Арматура воздуховодов";
                case BuiltInCategory.OST_DuctTerminal:
                    return "3. Воздухораспределители";
                case BuiltInCategory.OST_DuctCurves:
                    return "4. Воздуховоды";
                case BuiltInCategory.OST_FlexDuctCurves:
                    return "4. Гибкие воздуховоды";
                case BuiltInCategory.OST_DuctFitting:
                    return "5. Фасонные детали воздуховодов";
                case BuiltInCategory.OST_DuctInsulations:
                    return "6. Материалы изоляции воздуховодов";
                case BuiltInCategory.OST_PipeAccessory:
                    return "7. Трубопроводная арматура";
                case BuiltInCategory.OST_PipeCurves:
                    return "8. Трубопроводы";
                case BuiltInCategory.OST_FlexPipeCurves:
                    return "9. Гибкие трубопроводы";
                case BuiltInCategory.OST_PipeFitting:
                    return "10. Фасонные детали трубопроводов";
                case BuiltInCategory.OST_PipeInsulations:
                    return "11. Материалы трубопроводной изоляции";
            }

            return "Неизвестная категория";
        }

        /// <summary>
        /// Получаем детализированную группу - имя + марка + код + завод
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetDetailedGroup(Element element) {
            Element elemType = element.GetElementType();

            string name = GetName(element, elemType);
            string mark = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameMark);
            string code = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameCode);
            string creator = element.GetTypeOrInstanceParamStringValue(elemType, _config.OriginalParamNameCreator);
            return $"_{name}_{mark}_{code}_{creator}";
        }
    }
}
