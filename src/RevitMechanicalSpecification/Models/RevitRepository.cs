using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using dosymep.Revit;
using RevitMechanicalSpecification.Models.Fillers;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties;
using System.Xml.Linq;
using Autodesk.Revit.DB.Mechanical;
using RevitMechanicalSpecification.Service;
using System.Diagnostics;
using System;
using RevitMechanicalSpecification.Entities;



namespace RevitMechanicalSpecification.Models {
    internal class RevitRepository {

        private readonly ElementProcessor _elementProcessor;
        private readonly List<ElementParamFiller> _fillersSpecRefresh;
        private readonly List<ElementParamFiller> _fillersSystemRefresh;
        private readonly List<ElementParamFiller> _fillersFunctionRefresh;
        private readonly CollectionFactory _collector;
        private List<Element> _elements;
        private readonly List<VisSystem> _visSystems;
        private readonly SpecConfiguration _specConfiguration;
        private readonly VisElementsCalculator _calculator;
        private readonly MaskReplacer _maskReplacer;


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            _elementProcessor = new ElementProcessor(UIApplication.Application.Username, Document);
            _specConfiguration = new SpecConfiguration(Document.ProjectInformation);
            _collector = new CollectionFactory(Document, _specConfiguration, ActiveUIDocument);
            _calculator = new VisElementsCalculator(_specConfiguration, Document);
            _maskReplacer = new MaskReplacer(_specConfiguration);

            _visSystems = _collector.GetVisSystems();

            _fillersSpecRefresh = new List<ElementParamFiller>()
{
                //Заполнение ФОП_ВИС_Наименование комбинированное
                new ElementParamNameFiller(
                _specConfiguration.TargetNameName,
                _specConfiguration.OriginalParamNameName,
                _specConfiguration,
                Document,
                _calculator),
                //Заполнение ФОП_ВИС_Марка
                new ElementParamMarkFiller(
                _specConfiguration.TargetNameMark,
                _specConfiguration.OriginalParamNameMark,
                _specConfiguration,
                _calculator,
                Document),
                //Заполнение ФОП_ВИС_Группирование
                new ElementParamGroupFiller(
                _specConfiguration.TargetNameGroup,
                null,
                _specConfiguration,
                Document),

                //Заполнение ФОП_ВИС_Код изделия
                new ElementParamDefaultFiller(
                _specConfiguration.TargetNameCode,
                _specConfiguration.OriginalParamNameCode,
                _specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Завод-изготовитель
                new ElementParamDefaultFiller(
                _specConfiguration.TargetNameCreator,
                _specConfiguration.OriginalParamNameCreator,
                _specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Единица измерения
                new ElementParamUnitFiller(
                _specConfiguration.TargetNameUnit,
                _specConfiguration.OriginalParamNameUnit,
                _specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Число - у него нет исходного параметра, набор идет из системных, так что на вход идет null
                new ElementParamNumberFiller(
                _specConfiguration.TargetNameNumber,
                null,
                _specConfiguration,
                Document),

        };
            _fillersSystemRefresh = new List<ElementParamFiller>()
            { 
                //Заполнение ФОП_ВИС_Имя системы
                new ElementParamSystemFiller(
                _specConfiguration.TargetNameSystem,
                null,
                _specConfiguration,
                Document,
                _visSystems)
            };
            _fillersFunctionRefresh = new List<ElementParamFiller>()
            { 
                //Заполнение ФОП_ВИС_Экономическая функция
                new ElementParamFunctionFiller(
                _specConfiguration.TargetNameFunction,
                null,
                _specConfiguration,
                Document,
                _visSystems)
            };
        }


        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;

        /// <summary>
        /// Обновление только по филлерам спецификации
        /// </summary>
        public void SpecificationRefresh() {
            _elements = _collector.GetElementsToSpecificate();
            _elementProcessor.ProcessElements(_fillersSpecRefresh, _elements);
        }

        /// <summary>
        /// Обновление только по филлерам системы
        /// </summary>
        public void RefreshSystemName() {
            _elements = _collector.GetElementsToSpecificate();
            _elementProcessor.ProcessElements(_fillersSystemRefresh, _elements);
        }

        /// <summary>
        /// Обновление только по филлерам функции
        /// </summary>
        public void RefreshSystemFunction() {
            _elements = _collector.GetElementsToSpecificate();
            _elementProcessor.ProcessElements(_fillersFunctionRefresh, _elements);
        }

        /// <summary>
        /// Здесь нужно провести полное обновление всех параметров, поэтому будут сложены все филлеры в один лист 
        /// </summary>
        public void FullRefresh(bool visible = false, bool selected = false) {
            _elements = _collector.GetElementsToSpecificate(visible, selected);
            List<ElementParamFiller> fillers = new List<ElementParamFiller>();
            fillers.AddRange(_fillersSpecRefresh);
            fillers.AddRange(_fillersFunctionRefresh);
            fillers.AddRange(_fillersSystemRefresh);
            _elementProcessor.ProcessElements(fillers, _elements);
        }

        /// <summary>
        /// Вызов замены маски в шаблонизированных семействах-генериках. Отдельный мини-плагин, который должен вызываться
        /// вместе с спекой, поэтому проще его встроить сюда
        /// </summary>
        public void ReplaceMask() {
            using(var t = Document.StartTransaction("Сформировать имя")) {
                foreach(Element element in _elements) {
                    _maskReplacer.ExecuteReplacment(element);
                }
                t.Commit();
            }
        }

    }
}
