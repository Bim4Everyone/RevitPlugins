using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    internal class ElementParamNameFiller : ElementParamFiller {
        private readonly VisElementsCalculator _calculator;
        private string _name;
        private string _nameAddon;


        public ElementParamNameFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document,
            VisElementsCalculator visElementsCalculator) :
            base(toParamName, fromParamName, specConfiguration, document) {
            _calculator = visElementsCalculator;
        }

        public override void SetParamValue(SpecificationElement specificationElement) {
            string name = GetName(specificationElement);

            TargetParameter.Set(specificationElement.ManifoldInstance != null
                ? $"‎    •  {name}"
                : specificationElement.Element.GetSharedParamValueOrDefault(Config.ForcedName, name));

            specificationElement.ElementName = name;
        }

        /// <summary>
        /// Получает имена элементов, в формате имя + дополнение к имени. Отдельно в switch-case обрабатываются особенные
        /// категории
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetName(SpecificationElement specificationElement) {
            _name = specificationElement.GetTypeOrInstanceParamStringValue(Config.OriginalParamNameName);
            _nameAddon = specificationElement.GetTypeOrInstanceParamStringValue(Config.NameAddition);

            if(string.IsNullOrEmpty(_name)) {
                _name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ";
            }
            switch(specificationElement.BuiltInCategory) {
                case BuiltInCategory.OST_DuctCurves:
                    return GetDuctName(specificationElement);
                case BuiltInCategory.OST_DuctFitting:
                    return GetDuctFittingName(specificationElement);
                case BuiltInCategory.OST_PipeFitting:
                    return GetPipeFittingName(specificationElement);
                case BuiltInCategory.OST_PipeCurves:
                    return GetPipeName(specificationElement.Element, specificationElement.ElementType);
                case BuiltInCategory.OST_PipeInsulations:
                    return GetPipeInsulationName(specificationElement);
            }

            return $"{_name} {_nameAddon}";
        }

        /// <summary>
        /// Возвращает имя воздуховода
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetDuctName(SpecificationElement specificationElement) {
            return
            $"{_name} " +
            $"{_calculator.GetDuctName(specificationElement.Element, specificationElement.ElementType)} " +
            $"{_nameAddon}";
        }

        /// <summary>
        /// Возвращает имя соединительной детали трубопровода
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetDuctFittingName(SpecificationElement specificationElement) {
            return $"{_calculator.GetDuctFittingName(specificationElement.Element)} {_nameAddon}";
        }

        /// <summary>
        /// Возвращает имя соединительной детали воздуховода
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetPipeFittingName(SpecificationElement specificationElement) {
            // Если учет фитингов труб отключен в проекте, не учитываем. Если включен в проекте, но выключен в трубе - не учитываем
            return _calculator.IsSpecifyPipeFittingName(specificationElement.Element) ?
                $"{_name} {_nameAddon}" : "!Не учитывать";
        }

        /// <summary>
        /// Возвращает имя трубы. Принимаем не SpecificationElement, потому что отдельно этот метод вызывается из
        /// имени изоляции, где нужно будет сюда посылать ее хоста
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="elemType"></param>
        /// <returns></returns>
        private string GetPipeName(Element pipe, Element elemType) {
            string name = pipe.GetTypeOrInstanceParamStringValue(elemType, Config.OriginalParamNameName);
            string nameAddon = pipe.GetTypeOrInstanceParamStringValue(elemType, Config.NameAddition);

            if(string.IsNullOrEmpty(name)) {
                name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ";
            }
            return $"{name} {_calculator.GetPipeSize(pipe, elemType)} {nameAddon}";
        }

        /// <summary>
        /// Возвращает имя изоляции трубы и имя самой трубы.
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetPipeInsulationName(SpecificationElement specificationElement) {
            InsulationLiningBase insulation = specificationElement.Element as InsulationLiningBase;
            Element pipe = Document.GetElement(insulation.HostElementId);
            return (pipe != null & pipe.Category.IsId(BuiltInCategory.OST_PipeCurves)) ?
                $"{_name} " +
                $"(Для: {GetPipeName(pipe, pipe.GetElementType())}) {_nameAddon}"
                : "!Не учитывать";
        }
    }
}
