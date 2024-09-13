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

        internal HashSet<ManifoldPart> ManifoldParts;
        private readonly List<ElementParamFiller> _fillersSpecRefresh;
        private readonly List<ElementParamFiller> _fillersSystemRefresh;
        private readonly List<ElementParamFiller> _fillersFunctionRefresh;
        private readonly NameGroupFactory _nameAndGroupFactory;
        private readonly CollectionFactory _collector;
        private readonly List<Element> _elements;
        private readonly List<VisSystem> _visSystems;
        private readonly SpecConfiguration _specConfiguration;
        private readonly VisElementsCalculator _calculator;
        private readonly ParameterChecker _parameterChecker;
        private readonly MaskReplacer _maskReplacer;

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

            ManifoldParts = new HashSet<ManifoldPart>();

            _elements = _collector.GetMechanicalElements();
            _visSystems = _collector.GetMechanicalSystemColl();
            _specConfiguration = new SpecConfiguration(Document.ProjectInformation);
            _collector = new CollectionFactory(Document, _specConfiguration);
            _calculator = new VisElementsCalculator(_specConfiguration, Document);
            _nameAndGroupFactory = new NameGroupFactory(_specConfiguration, Document, _calculator);
            _parameterChecker = new ParameterChecker();
            _maskReplacer = new MaskReplacer(_specConfiguration);

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

        /// <summary>
        /// Обновление только по филлерам спецификации
        /// </summary>
        public void SpecificationRefresh() {
            ProcessElements(_fillersSpecRefresh);
        }

        /// <summary>
        /// Обновление только по филлерам системы
        /// </summary>
        public void RefreshSystemName() {
            ProcessElements(_fillersSystemRefresh);
        }

        /// <summary>
        /// Обновление только по филлерам функции
        /// </summary>
        public void RefreshSystemFunction() {
            ProcessElements(_fillersFunctionRefresh);
        }

        /// <summary>
        /// Здесь нужно провести полное обновление всех параметров, поэтому будут сложены все филлеры в один лист 
        /// </summary>
        public void FullRefresh() {
            List<ElementParamFiller> fillers = new List<ElementParamFiller>();
            fillers.AddRange(_fillersSpecRefresh);
            fillers.AddRange(_fillersFunctionRefresh);
            fillers.AddRange(_fillersSystemRefresh);
            ProcessElements(fillers);
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

        /// <summary>
        /// Выводит отчет с занявшими элемент пользователями
        /// </summary>
        /// <param name="editors"></param>
        public void ShowReport(List<string> editors) {
            if(editors.Count != 0) {
                MessageBox.Show("Некоторые элементы не были обработаны, так как заняты пользователем/пользователями: "
                    + string.Join(", ", editors.ToArray()));
            }
        }

        /// <summary>
        /// Проверка на занятость элемента
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="element"></param>
        /// <returns></returns>
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

        /// <summary>
        /// После того как сабэлементы узла были отработаны филлерами - закидываем их в экземпляры класса части узла,
        /// чтоб при следующей встрече в узловой обработке иметь возможность проигнорировать при встрече в обработке вне узловой
        /// </summary>
        /// <param name="element"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private ManifoldPart CreateManifoldParts(Element element, string group) {
            Element elemType = element.GetElementType();
            if(!_nameAndGroupFactory.IsOutSideOfManifold(elemType)) {
                ManifoldPart part = new ManifoldPart {
                    Group = group,
                    Id = element.Id
                };
                return part;
            }
            return null;
        }

        /// <summary>
        /// Главный цикл обработки, проверяет в себе занятость элементов, генерики и узел это или нет
        /// </summary>
        /// <param name="fillers"></param>
        private void ProcessElements(List<ElementParamFiller> fillers) {
            _parameterChecker.ExecuteParameterCheck(Document);

            string userName = UIApplication.Application.Username.ToLower();
            List<string> editors = new List<string>();

            using(var t = Document.StartTransaction("Обновление спецификации")) {
                foreach(Element element in _elements) {
                    // Это должна быть всегда первая обработка. Если элемент на редактировании - идем дальше, записав 
                    // редактора в список
                    string editor = IsEditedBy(userName, element);
                    if(!string.IsNullOrEmpty(editor)) {
                        editors.Add(editor);
                        continue;
                    }

                    // На арматуре воздуховодов/труб/оборудовании проверяем наличие шаблонизированных семейств-генериков.
                    // Если встречаем - заполняем все по маске
                    if(element.InAnyCategory(new HashSet<BuiltInCategory>() {
                        BuiltInCategory.OST_DuctAccessory,
                        BuiltInCategory.OST_PipeAccessory,
                        BuiltInCategory.OST_MechanicalEquipment})) {
                        bool maskExist = _maskReplacer.ExecuteReplacment(element);
                        if(maskExist) {
                            continue;
                        }
                    }

                    // Если элемент уже встречался в обработке вложений узлов - переходим к следующему
                    if(ManifoldParts.Any(part => part.Id == element.Id)) {
                        continue;
                    }

                    ProcessElement(element, fillers);
                    ProcessManifoldElement(element, fillers);
                }
                t.Commit();
                ShowReport(editors);
            }
        }

        /// <summary>
        /// Обработка элемента филлерами, если в них нет пометки что это узел
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fillers"></param>
        private void ProcessElement(Element element, List<ElementParamFiller> fillers) {
            if(!_nameAndGroupFactory.IsManifold(element)) {
                foreach(var filler in fillers) {
                    filler.Fill(element);
                }
            }
        }

        /// <summary>
        /// Обработка филлерами узлов
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fillers"></param>
        private void ProcessManifoldElement(Element element, List<ElementParamFiller> fillers) {
            Element elemType = element.GetElementType();
            if(_nameAndGroupFactory.IsManifold(elemType)) {
                int positionNumber = 1;
                FamilyInstance familyInstance = element as FamilyInstance;

                // Сортируем лист по параметру группирования, чтоб присвоить нумерацию узлу от его индексов
                List<Element> manifoldElements = _nameAndGroupFactory.GetSub(familyInstance)
                    .OrderBy(e => _nameAndGroupFactory.GetGroup(e))
                    .Where(e => !_nameAndGroupFactory.IsOutSideOfManifold(e.GetElementType()))
                    .ToList();

                // Если не стоит галочка "Исключить из узла"(проверяется в сортировке выше),
                // проверяем меняется ли номер элемента в узле и отправляем элемент в филлеры
                foreach(Element subElement in manifoldElements) {
                    Element subElementType = subElement.GetElementType();
                    int index = manifoldElements.IndexOf(subElement);

                    if(_nameAndGroupFactory.IsIncreaseCount(manifoldElements, index, subElement, subElementType)) {
                        positionNumber++;
                    }

                    ProcessManifoldSubElement(fillers, subElement, familyInstance, positionNumber);
                }

                // После того как сабэлементы узла были отработаны филлерами - закидываем их в экземпляры класса части узла,
                // чтоб при следующей встрече в узловой обработке иметь возможность проигнорировать при встрече в обработке вне узловой
                foreach(Element subElement in manifoldElements) {
                    string group = _nameAndGroupFactory.GetGroup(subElement);
                    ManifoldPart part = CreateManifoldParts(subElement, group);

                    if(part != null) {
                        ManifoldParts.Add(part);
                    }
                }
            }
        }

        /// <summary>
        /// Обработка филлерами вложенного элемента узла с простановкой ему нумерации внутри узла
        /// </summary>
        /// <param name="fillers"></param>
        /// <param name="manifoldElement"></param>
        /// <param name="familyInstance"></param>
        /// <param name="count"></param>
        private void ProcessManifoldSubElement(
            List<ElementParamFiller> fillers,
            Element manifoldElement,
            FamilyInstance familyInstance,
            int count) {
            foreach(var filler in fillers) {
                filler.Fill(manifoldElement, familyInstance, count, ManifoldParts);
            }
        }


    }
}
