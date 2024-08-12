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
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;


            ManifoldParts = new HashSet<ManifoldPart> { };
            _collector = new CollectionFactory(Document);
            _elements = _collector.GetMechanicalElements();
            _visSystems = _collector.GetMechanicalSystemColl();
            _specConfiguration = new SpecConfiguration(Document.ProjectInformation);
            _calculator = new VisElementsCalculator(_specConfiguration, Document);
            _nameAndGroupFactory = new NameAndGroupFactory(_specConfiguration, Document, _calculator);
            _fillersSpecRefresh = new List<ElementParamFiller>()
{
                //Заполнение ФОП_ВИС_Группирование
                new ElementParamGroupFiller(
                _specConfiguration.TargetNameGroup,
                null,
                _specConfiguration,
                Document,
                _nameAndGroupFactory),
                //Заполнение ФОП_ВИС_Марка
                new ElementParamMarkFiller(
                _specConfiguration.TargetNameMark,
                _specConfiguration.OriginalParamNameMark,
                _specConfiguration,
                _calculator,
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
                Document,
                _nameAndGroupFactory),
                //Заполнение ФОП_ВИС_Наименование комбинированное
                new ElementParamNameFiller(
                _specConfiguration.TargetNameName,
                _specConfiguration.OriginalParamNameName,
                _specConfiguration,
                Document,
                _nameAndGroupFactory)
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


        private readonly List<ElementParamFiller> _fillersSpecRefresh;
        private readonly List<ElementParamFiller> _fillersSystemRefresh;
        private readonly List<ElementParamFiller> _fillersFunctionRefresh;


        internal HashSet<ManifoldPart> ManifoldParts;
        private readonly NameAndGroupFactory _nameAndGroupFactory;
        private readonly CollectionFactory _collector;
        private readonly List<Element> _elements;
        private readonly List<VisSystem> _visSystems;
        private readonly SpecConfiguration _specConfiguration;
        private readonly VisElementsCalculator _calculator;

        public void ShowReport(List<string> editors) {
            if(editors.Count != 0) {
                MessageBox.Show("Некоторые элементы не были обработаны, так как заняты пользователем/пользователями: "
                    + string.Join(", ", editors.ToArray()));
            }
        }

        private string IsEditedBy(string userName, Element element) {

            string editedBy = element.GetParamValueOrDefault<string>(BuiltInParameter.EDITED_BY);

            if(!string.IsNullOrEmpty(editedBy)) {
                editedBy = editedBy.ToLower();
                if(editedBy != userName) {
                    return editedBy;
                }
            }
            return null;
        }

        private ManifoldPart CreateManifoldParts(Element element, string group) {
            Element elemType = element.GetElementType();
            if(!_nameAndGroupFactory.IsOutSideOfManifold(elemType)) {
                ManifoldPart part = new ManifoldPart();
                part.Group = group;
                part.Id = element.Id;
                return part;
            }
            return null;
        }

        private void ProcessElements(List<ElementParamFiller> fillers) {
            string userName = UIApplication.Application.Username.ToLower();
            List<string> editors = new List<string>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            using(var t = Document.StartTransaction("Обновление спецификации")) {
                foreach(var element in _elements) {
                    string editor = IsEditedBy(userName, element);
                    if(!string.IsNullOrEmpty(editor)) {
                        if(!editors.Contains(editor)) {
                            editors.Add(editor);
                        }
                        continue;
                    }

                    if(ManifoldParts.Where(part => part.Id == element.Id).ToHashSet().Count > 0) {
                        continue;
                    };

                    ProcessElement(element, fillers);
                    ProcessManifoldElement(element, fillers);
                }

                t.Commit();

                ShowReport(editors);

                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                MessageBox.Show(elapsedTime);

            }
        }

        private void ProcessElement(Element element, List<ElementParamFiller> fillers) {
            if(!_nameAndGroupFactory.IsManifold(element)) {
                foreach(var filler in fillers) {
                    filler.Fill(element);
                }
            }
        }

        private void ProcessManifoldElement(Element element, List<ElementParamFiller> fillers) {
            Element elemType = element.GetElementType();
            if(_nameAndGroupFactory.IsManifold(elemType)) {
                int positionNumber = 1;
                FamilyInstance familyInstance = element as FamilyInstance;

                //Сортируем по параметру группирования, чтоб присвоить нумерацию узлу
                List<Element> manifoldElements = _nameAndGroupFactory.GetSub(familyInstance)
                    .OrderBy(e => _nameAndGroupFactory.GetGroup(e))
                    .Where(e => !_nameAndGroupFactory.IsOutSideOfManifold(e.GetElementType()))
                    .ToList();

                //Если не стоит галочка "Исключить из узла", проверяем меняется ли номер элемента в узле и отправляем элемент в филлеры
                foreach(Element subElement in manifoldElements) {
                    Element subElementType = subElement.GetElementType();
                    int index = manifoldElements.IndexOf(subElement);

                    if(_nameAndGroupFactory.IsIncreaseCount(manifoldElements, index, subElement, subElementType)) {
                        positionNumber++;
                    }

                    ProcessManifoldSubElement(fillers, subElement, familyInstance, positionNumber);
                }

                //После того как сабэлементы узла были отработаны филлерами - закидываем их в экземпляры класса части узла,
                //чтоб при следующей встрече в узловой обработке иметь возможность занулить их число в филлере числа
                //И проигнорировать при встрече в обработке вне узловой
                foreach(Element subElement in manifoldElements) {
                    string group = _nameAndGroupFactory.GetGroup(subElement);
                    ManifoldPart part = CreateManifoldParts(subElement, group);

                    if(part != null) {
                        ManifoldParts.Add(part);
                    }
                }
            }
        }

        private void ProcessManifoldSubElement(
            List<ElementParamFiller> fillers,
            Element manifoldElement,
            FamilyInstance familyInstance,
            int count) {
            foreach(var filler in fillers) {
                filler.Fill(manifoldElement, familyInstance, count, ManifoldParts);
            }
        }

        public void SpecificationRefresh() {
            ProcessElements(_fillersSpecRefresh);
        }

        public void RefreshSystemName() {
            ProcessElements(_fillersSystemRefresh);
        }

        public void RefreshSystemFunction() {
            ProcessElements(_fillersFunctionRefresh);
        }

        public void FullRefresh() {
            List<ElementParamFiller> fillers = new List<ElementParamFiller>();
            fillers.AddRange(_fillersSpecRefresh);
            fillers.AddRange(_fillersFunctionRefresh);
            fillers.AddRange(_fillersSystemRefresh);
            ProcessElements(fillers);
        }

        public void ReplaceMask() {
            using(var t = Document.StartTransaction("Сформировать имя")) {
                foreach(Element element in _elements) {
                    MaskReplacer.ReplaceMask(element, _specConfiguration.MaskMarkName, "ADSK_Марка");
                    MaskReplacer.ReplaceMask(element, _specConfiguration.MaskNameName, "ADSK_Наименование");
                }
                t.Commit();
            }
        }
    }
}
