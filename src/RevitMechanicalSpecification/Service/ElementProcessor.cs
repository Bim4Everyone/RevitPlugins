using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Models.Fillers;
using RevitMechanicalSpecification.Models;
using dosymep.Revit;
using System.Windows;

namespace RevitMechanicalSpecification.Service {
    internal class ElementProcessor {
        private readonly string _userName;
        private readonly Document _document;
        private readonly SpecConfiguration _specConfiguration;
        private readonly ParamChecker _paramChecker;
        private readonly MaskReplacer _maskReplacer;
        private readonly List<string> _editors;
        private readonly HashSet<BuiltInCategory> _possibleGenericCategories = new HashSet<BuiltInCategory>() {
                        BuiltInCategory.OST_DuctAccessory,
                        BuiltInCategory.OST_PipeAccessory,
                        BuiltInCategory.OST_MechanicalEquipment
    };

        public ElementProcessor(string userName, Document document) {
            _userName = userName;
            _document = document;

            _specConfiguration = new SpecConfiguration(_document.ProjectInformation);
            _paramChecker = new ParamChecker();
            _maskReplacer = new MaskReplacer(_specConfiguration);
            _editors = new List<string>();

        }

        /// <summary>
        /// Главный цикл обработки, проверяет в себе занятость элементов, генерики и узел это или нет
        /// </summary>
        /// <param name="fillers"></param>
        public void ProcessElements(List<ElementParamFiller> fillers, List<Element> elements) {
            _paramChecker.ExecuteParamCheck(_document, _specConfiguration);

            using(var t = _document.StartTransaction("Обновление спецификации")) {
                ElementSplitResult splitResult = SplitElementsToManifoldOrSingle(elements);

                foreach(SpecificationElement specificationElement in splitResult.SingleElements) {
                    ProcessElement(specificationElement, fillers);
                }

                foreach(SpecificationElement manifoldElement in splitResult.ManifoldElements) {
                    // В этот момент мы уже прошлись по одиночным элементам и у них есть имена, которые нужны в группирование, получаем их
                    manifoldElement.ManifoldSpElement.ElementName = 
                        manifoldElement.ManifoldSpElement
                        .GetTypeOrInstanceParamStringValue(_specConfiguration.TargetNameName);
                    ProcessElement(manifoldElement, fillers);
                }
                t.Commit();
                ShowReport();
            }
        }

        /// <summary>
        /// Если генерик - заполняет его и возвращает True. Иначе возвращает False.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool FillIfGeneric(Element element) {
            if(element.InAnyCategory(_possibleGenericCategories)) {
                return _maskReplacer.ExecuteReplacment(element);
            }
            return false;
        }

        /// <summary>
        /// Обработка элемента филлерами, если в них нет пометки что это узел
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fillers"></param>
        private void ProcessElement(SpecificationElement specificationElement, List<ElementParamFiller> fillers) {
            foreach(var filler in fillers) {
                filler.Fill(specificationElement);
            }
        }

        /// <summary>
        /// Проверяем значение галочки "ФОП_ВИС_Узел"
        /// </summary>
        /// <param name="elemType"></param>
        /// <returns></returns>
        private bool IsManifold(Element elemType) {
            return elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.IsManiFoldParamName) == 1;
        }

        /// <summary>
        /// Проверяем значение галочки "ФОП_ВИС_Исключить из узла"
        /// </summary>
        /// <param name="elemType"></param>
        /// <returns></returns>
        private bool IsOutSideOfManifold(Element elemType) {
            return elemType.GetSharedParamValueOrDefault<int>(_specConfiguration.IsOutSideOfManifold) == 1;
        }

        /// <summary>
        /// Выводит отчет с занявшими элемент пользователями
        /// </summary>
        /// <param name="editors"></param>
        public void ShowReport() {
            if(_editors.Count != 0) {
                MessageBox.Show("Некоторые элементы не были обработаны, так как заняты пользователем/пользователями: "
                    + string.Join(", ", _editors.ToArray()));
            }
        }

        /// <summary>
        /// Проверка на занятость элемента
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsEditedBy(string userName, Element element) {
            string editedBy = element.GetParamValueOrDefault<string>(BuiltInParameter.EDITED_BY);

            if(string.IsNullOrEmpty(editedBy)) {
                return false;
            }

            if(!string.Equals(editedBy, userName, StringComparison.OrdinalIgnoreCase)) {
                if(!_editors.Contains(editedBy)) {
                    _editors.Add(editedBy);
                }
                return true;
            }

            return false;
        }


        private ElementSplitResult SplitElementsToManifoldOrSingle(List<Element> elements) {
            HashSet<ElementId> manifoldPartsIds = new HashSet<ElementId>();
            List<SpecificationElement> singleElements = new List<SpecificationElement>();
            List<SpecificationElement> manifoldElements = new List<SpecificationElement>();

            foreach(Element element in elements) {
                // Это должна быть всегда первая обработка. Если элемент на редактировании - идем дальше, записав 
                // редактора в список
                if(IsEditedBy(_userName, element)) {
                    continue;
                }

                // На арматуре воздуховодов/труб/оборудовании проверяем наличие шаблонизированных семейств-генериков.
                // Если встречаем - заполняем все по маске
                if(FillIfGeneric(element)) {
                    continue;
                }

                if(manifoldPartsIds.Contains(element.Id)) {
                    continue;
                }

                SpecificationElement specificationElement = CreateSpecificationElement(element);

                if(IsManifold(specificationElement.ElementType)) {
                    ProcessManifoldElement(specificationElement, manifoldPartsIds, manifoldElements);
                } else {
                    singleElements.Add(specificationElement);
                }
            }

            return new ElementSplitResult(singleElements, manifoldElements);
        }

        private SpecificationElement CreateSpecificationElement(Element element) {
            return new SpecificationElement {
                Element = element,
                ElementType = element.GetElementType(),
                BuiltInCategory = element.Category.GetBuiltInCategory()
            };
        }

        private void ProcessManifoldElement(SpecificationElement specificationElement,
            HashSet<ElementId> manifoldPartsIds,
            List<SpecificationElement> manifoldElements) {
            FamilyInstance familyInstance = specificationElement.Element as FamilyInstance;
            List<Element> subElements = DataOperator.GetSub(familyInstance, _document);

            foreach(Element subElement in subElements) {
                Element subElementType = subElement.GetElementType();
                if(IsOutSideOfManifold(subElementType)) {
                    continue;
                }

                manifoldPartsIds.Add(subElement.Id);

                SpecificationElement subSpecificationElement = CreateSubSpecificationElement(subElement, familyInstance, specificationElement);
                manifoldElements.Add(subSpecificationElement);
            }
        }

        private SpecificationElement CreateSubSpecificationElement(Element subElement, FamilyInstance familyInstance, SpecificationElement parentSpecificationElement) {
            return new SpecificationElement {
                Element = subElement,
                ElementType = subElement.GetElementType(),
                ManifoldInstance = familyInstance,
                ManifoldSpElement = parentSpecificationElement,
                BuiltInCategory = subElement.Category.GetBuiltInCategory()
            };
        }
    }
}
