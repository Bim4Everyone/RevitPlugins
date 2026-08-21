using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMechanicalSpecification.Entities;
using RevitMechanicalSpecification.Models.Fillers;
using RevitMechanicalSpecification.Models;
using dosymep.Revit;
using System.Threading;
using dosymep.SimpleServices;
using dosymep.Xpf.Core.SimpleServices;
using dosymep.WPF.ViewModels;
using dosymep.Bim4Everyone.SimpleServices;
using System.Diagnostics;


namespace RevitMechanicalSpecification.Service {
    internal class ElementProcessor {
        private readonly Document _document;
        private readonly SpecConfiguration _specConfiguration;
        private readonly ElementEditChecker _elementEditChecker;
        private readonly DuctRotationCorrector _ductRotationCorrector;
        private readonly ParamChecker _paramChecker;
        private readonly MaskReplacer _maskReplacer;

        private readonly HashSet<BuiltInCategory> _insulationCategories = new HashSet<BuiltInCategory>() {
                        BuiltInCategory.OST_DuctInsulations,
                        BuiltInCategory.OST_PipeInsulations
    };

        public ElementProcessor(
            Document document,
            SpecConfiguration specConfiguration,
            ElementEditChecker elementEditChecker,
            DuctRotationCorrector ductRotationCorrector) {
            _document = document;

            _specConfiguration = specConfiguration;
            _elementEditChecker = elementEditChecker;
            _ductRotationCorrector = ductRotationCorrector;
            _paramChecker = new ParamChecker(_elementEditChecker);
            _maskReplacer = new MaskReplacer(_specConfiguration);
        }

        /// <summary>
        /// Вывод на экран процесса обработки элементов, отсюда же вызываем главный цикл обработки
        /// </summary>
        /// <param name="fillers"></param>
        /// <param name="elements"></param>
        public void ShowProcess(List<ElementParamFiller> fillers,
            List<Element> elements) {
            ElementSplitResult splitResult = SplitElementsToManifoldOrSingle(elements);

            //ProcessElements(splitResult, fillers);
            using(IProgressDialogService dialog = ServicesProvider.GetPlatformService<IProgressDialogService>()) {


                dialog.StepValue = 1;
                dialog.DisplayTitleFormat = "Обновление параметров... [{0}%]";
                var progress = dialog.CreateProgress();
                dialog.MaxValue = 100;
                var ct = dialog.CreateCancellationToken();
                dialog.Show();

                ProcessElements(splitResult, fillers, progress, ct);
            }
        }

        /// <summary>
        /// Главный цикл обработки, проверяет в себе занятость элементов, генерики и узел это или нет
        /// </summary>
        /// <param name="fillers"></param>
        public void ProcessElements(
        ElementSplitResult splitResult,
        List<ElementParamFiller> fillers,
        IProgress<int> progress = null,
        CancellationToken ct = default) {
            _paramChecker.ExecuteParamCheck(_document, _specConfiguration);

            using(var t = _document.StartTransaction("Обновление спецификации")) {
                _ductRotationCorrector.Execute(splitResult);

                var totalElements = splitResult.SingleElements.Count + splitResult.ManifoldElements.Count;
                
                var percent = totalElements * 0.01; //1 процент от элементов
                var nextStepByPercents = totalElements * 0.01; // число элементов по достижению которых обновляем счетчик
                var elementCount = 0; // счетчик элементов
                var percentCount = 0; // счетчик процентов, на него умножаем nextStepByPercents для обновления условия

                foreach(SpecificationElement specificationElement in splitResult.SingleElements) {
                    ct.ThrowIfCancellationRequested();

                    elementCount++;
                    if(elementCount > nextStepByPercents) {
                        progress.Report(percentCount);
                        percentCount += 1;
                        nextStepByPercents = percent * percentCount;
                    }
                    
                    _maskReplacer.ExecuteReplacment(specificationElement);
                    ProcessElement(specificationElement, fillers);
                }

                foreach(SpecificationElement manifoldElement in splitResult.ManifoldElements) {
                    // В этот момент мы уже прошлись по одиночным элементам и у них есть имена, которые нужны в группирование, получаем их
                    manifoldElement.ManifoldSpElement.ElementName =
                        manifoldElement.ManifoldSpElement
                        .GetTypeOrInstanceParamStringValue(_specConfiguration.TargetNameName);

                    if(elementCount > nextStepByPercents) {
                        progress.Report(percentCount);
                        percentCount += 1;
                        nextStepByPercents = percent * percentCount;
                    }
                    _maskReplacer.ExecuteReplacment(manifoldElement);
                    ProcessElement(manifoldElement, fillers);
                }
                t.Commit();
                _elementEditChecker.ShowReport();
                _ductRotationCorrector.ShowReport();
            }
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
        /// Делим исходный список элементов на узловые и одиночные, сразу же превращая их в SpecificationElement
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private ElementSplitResult SplitElementsToManifoldOrSingle(List<Element> elements) {
            // Список элементов узлов в модели. Если элемент уже добавлен сюда - отдельно не обрабатываем,
            // он уже в нужном списке
            HashSet<ElementId> manifoldPartsIds = new HashSet<ElementId>();

            List<SpecificationElement> singleElements = new List<SpecificationElement>();
            List<SpecificationElement> manifoldElements = new List<SpecificationElement>();

            foreach(Element element in elements) {

                // Это должна быть всегда первая обработка. Если элемент недоступен для редактирования - идем дальше
                if(_elementEditChecker.IsUnavailableForEdit(element)) {
                    continue;
                }

                if(manifoldPartsIds.Contains(element.Id)) {
                    continue;
                }

                SpecificationElement specificationElement = CreateSpecificationElement(element);

                if(IsManifold(specificationElement.ElementType)) {
                    ProcessManifoldElement(specificationElement, manifoldPartsIds, manifoldElements);
                }

                singleElements.Add(specificationElement);
            }

            return new ElementSplitResult(singleElements, manifoldElements);
        }

        /// <summary>
        /// Создать SpecificationElement для одиночного элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private SpecificationElement CreateSpecificationElement(Element element) {
            SpecificationElement specificationElement = new SpecificationElement {
                Element = element,
                ElementType = element.GetElementType(),
                BuiltInCategory = element.Category.GetBuiltInCategory(),
                InsulationSpHost = null
            };

            if(element.InAnyCategory(_insulationCategories)) {
                specificationElement.InsulationSpHost = GetInsulationHost(element);
            }

            return specificationElement;
        }

        /// <summary>
        /// Возвращает SpecificationElement для хоста изоляции или none, если 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private SpecificationElement GetInsulationHost(Element element) {
            InsulationLiningBase insulation = element as InsulationLiningBase;

            Element host = _document.GetElement(insulation.HostElementId);
            // изоляция может баговать и висеть на трубе или воздуховоде не имея хоста
            if(host == null) {
                return null;
            }

            return new SpecificationElement {
                Element = host,
                ElementType = host.GetElementType(),
                BuiltInCategory = host.Category.GetBuiltInCategory()
            };
        }

        /// <summary>
        /// Создаем на базе specificationElement в котором есть галочка ФОП_ВИС_Узел specificationElement для узловых элементов,
        /// отмечаем их в списке айди для дальнейшего игнорирования и заполняем список узловых элементов
        /// </summary>
        /// <param name="specificationElement"></param>
        /// <param name="manifoldPartsIds"></param>
        /// <param name="manifoldElements"></param>
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

                if(_elementEditChecker.IsUnavailableForEdit(subElement)) {
                    continue;
                }

                SpecificationElement subSpecificationElement = CreateSubSpecificationElement(subElement, familyInstance, specificationElement);
                manifoldElements.Add(subSpecificationElement);
            }
        }

        /// <summary>
        /// Создать SpecificationElement для узлового элемента
        /// </summary>
        /// <param name="subElement"></param>
        /// <param name="familyInstance"></param>
        /// <param name="parentSpecificationElement"></param>
        /// <returns></returns>
        private SpecificationElement CreateSubSpecificationElement(Element subElement, FamilyInstance familyInstance, SpecificationElement parentSpecificationElement) {
            return new SpecificationElement {
                Element = subElement,
                ElementType = subElement.GetElementType(),
                ManifoldInstance = familyInstance,
                ManifoldSpElement = parentSpecificationElement,
                BuiltInCategory = subElement.Category.GetBuiltInCategory()
            };
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

    }
}
