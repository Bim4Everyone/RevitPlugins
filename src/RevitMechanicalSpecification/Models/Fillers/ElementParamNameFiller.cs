using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

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

            TargetParam.Set(specificationElement.ManifoldInstance != null
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
                case BuiltInCategory.OST_CableTray:
                    return GetCableTrayName(specificationElement);
                case BuiltInCategory.OST_FlexDuctCurves:
                    return GetFlexElementName(specificationElement);
                case BuiltInCategory.OST_FlexPipeCurves:
                    return GetFlexElementName(specificationElement);
            }

            return string.IsNullOrEmpty(_nameAddon) ? _name : $"{_name} {_nameAddon}";
        }

        /// <summary>
        /// Возвращает имя кабельного лотка
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetCableTrayName(SpecificationElement specificationElement) {
            double width = specificationElement
                .Element.GetParam(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM).AsDouble();
            width = UnitConverter.DoubleToMilimeters(width);

            double height = specificationElement
                .Element.GetParam(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM).AsDouble();
            height = UnitConverter.DoubleToMilimeters(height);

            return $"{_name} {width}x{height}";
        }

        /// <summary>
        /// Возвращает имя гибких воздуховодов и труб
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetFlexElementName(SpecificationElement specificationElement) {
            double diameter = 0;
            BuiltInParameter diameterParam = BuiltInParameter.INVALID;

            if(specificationElement.Element.Category.IsId(BuiltInCategory.OST_FlexPipeCurves)) {
                diameterParam = BuiltInParameter.RBS_PIPE_DIAMETER_PARAM;
            } else if(specificationElement.Element.Category.IsId(BuiltInCategory.OST_FlexDuctCurves)) {
                diameterParam = BuiltInParameter.RBS_CURVE_DIAMETER_PARAM;
            }

            if(diameterParam != BuiltInParameter.INVALID) {
                diameter = UnitConverter.DoubleToMilimeters(
                    specificationElement.Element.GetParam(diameterParam).AsDouble());
            }

            return $"{_name} ø{diameter}";
        }

        /// <summary>
        /// Проверяем существует ли bbox у элемента. Необходимо для того чтобы вычленять в спецификации позиции с сломанной геометрией.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsBboxNotExists(Element element) {
            return element.GetBoundingBox() == null;
        }

        /// <summary>
        /// Возвращает имя воздуховода
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetDuctName(SpecificationElement specificationElement) {
            if(IsBboxNotExists(specificationElement.Element)) {
                return "НЕКОРРЕКТНАЯ ГЕОМЕТРИЯ";
            }

            return
            $"{_name}" +
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

            if(IsBboxNotExists(pipe)) {
                return "НЕКОРРЕКТНАЯ ГЕОМЕТРИЯ";
            }

            if(string.IsNullOrEmpty(name)) {
                name = "ЗАПОЛНИТЕ НАИМЕНОВАНИЕ";
            }
            // Нужно возвращать без аддона, потому что мы вызываем это из получения имени изоляции
            // и там скобка идет с пробелом
            if(string.IsNullOrEmpty(nameAddon)) {
                return $"{name} {_calculator.GetPipeSize(pipe, elemType)}";
            }
            return $"{name} {_calculator.GetPipeSize(pipe, elemType)} {nameAddon}";
        }

        /// <summary>
        /// Возвращает имя изоляции трубы и имя самой трубы.
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <returns></returns>
        private string GetPipeInsulationName(SpecificationElement specificationElement) {
            InsulationLiningBase insulation = (InsulationLiningBase)specificationElement.Element;

            // Нужно проверить, что у изоляции реально есть хост. Изредка багует что его нет
            if(insulation.HostElementId.IsNull()) {
                return "!Не учиывать";
            }

            Element pipe = Document.GetElement(insulation.HostElementId);
            return (pipe != null & pipe.Category.IsId(BuiltInCategory.OST_PipeCurves)) ?
                $"{_name} " +
                $"(Для: {GetPipeName(pipe, pipe.GetElementType())}) {_nameAddon}"
                : "!Не учитывать";
        }
    }
}
