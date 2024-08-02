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



namespace RevitMechanicalSpecification.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

        }

        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;

        private List<ElementParamFiller> _fillers;

        public void ExecuteSpecificationRefresh() {
            CollectionFactory collector = new CollectionFactory(Document);
            List<Element> elements = collector.GetMechanicalElements();
            List<VisSystem> visSystems = collector.GetMechanicalSystemColl();

            SpecConfiguration specConfiguration = new SpecConfiguration(Document.ProjectInformation);
            VisElementsCalculator calculator = new VisElementsCalculator(specConfiguration, Document);
            NameAndGroupFactory nameAndGruopFactory = new NameAndGroupFactory(specConfiguration, Document, calculator);


            List<ElementParamFiller> testFillers = new List<ElementParamFiller>()
            {

                //Заполнение ФОП_ВИС_Группирование
                new ElementParamGroupFiller(
                specConfiguration.TargetNameGroup,
                null,
                specConfiguration,
                Document,
                nameAndGruopFactory),
                //Заполнение ФОП_ВИС_Наименование комбинированное
                new ElementParamNameFiller(
                specConfiguration.TargetNameName,
                specConfiguration.OriginalParamNameName,
                specConfiguration,
                Document,
                nameAndGruopFactory)

        };
            _fillers = new List<ElementParamFiller>()
            {
                //Заполнение ФОП_ВИС_Группирование
                new ElementParamGroupFiller(
                specConfiguration.TargetNameGroup,
                null,
                specConfiguration,
                Document,
                nameAndGruopFactory),
                //Заполнение ФОП_ВИС_Марка
                new ElementParamDefaultFiller(
                specConfiguration.TargetNameMark,
                specConfiguration.OriginalParamNameMark,
                specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Код изделия
                new ElementParamDefaultFiller(
                specConfiguration.TargetNameCode,
                specConfiguration.OriginalParamNameCode,
                specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Завод-изготовитель
                new ElementParamDefaultFiller(
                specConfiguration.TargetNameCreator,
                specConfiguration.OriginalParamNameCreator,
                specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Единица измерения
                new ElementParamUnitFiller(
                specConfiguration.TargetNameUnit,
                specConfiguration.OriginalParamNameUnit,
                specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Число - у него нет исходного параметра, набор идет из системных, так что на вход идет null
                new ElementParamNumberFiller(
                specConfiguration.TargetNameNumber,
                null,
                specConfiguration,
                Document),
                //Заполнение ФОП_ВИС_Наименование комбинированное
                new ElementParamNameFiller(
                specConfiguration.TargetNameName,
                specConfiguration.OriginalParamNameName,
                specConfiguration,
                Document,
                nameAndGruopFactory),
                //Заполнение ФОП_ВИС_Имя системы
                new ElementParamSystemFiller(
                specConfiguration.TargetNameSystem,
                null,
                specConfiguration,
                Document,
                visSystems),
                //Заполнение ФОП_ВИС_Экономическая функция
                new ElementParamSystemFiller(
                specConfiguration.TargetNameFunction,
                null,
                specConfiguration,
                Document,
                visSystems)

        };

            HashSet<ElementId> manifoldElementIds = new HashSet<ElementId>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            using(Transaction t = Document.StartTransaction("Обновление спецификации")) {


                foreach(Element element in elements) {
                    if(manifoldElementIds.Contains(element.Id))
                        continue;

                    Element elemType = element.GetElementType();

                    foreach(ElementParamFiller filler in _fillers) {
                        filler.Fill(element, null, 0);
                    }

                    if(nameAndGruopFactory.IsManifold(elemType)) {
                        int count = 1;
                        FamilyInstance familyInstance = element as FamilyInstance;
                        List<Element> manifoldElements =
                            nameAndGruopFactory.GetSub(familyInstance).OrderBy
                            (e => nameAndGruopFactory.GetGroup(e)).ToList();

                        foreach(Element manifoldElement in manifoldElements) {
                            Element maniElemType = manifoldElement.GetElementType();

                            if(!nameAndGruopFactory.IsOutSideOfManifold(maniElemType)) {
                                foreach(ElementParamFiller filler in _fillers) {
                                    filler.Fill(manifoldElement, familyInstance, count);
                                    manifoldElementIds.Add(manifoldElement.Id);

                                    if(nameAndGruopFactory.IsIncreaseIndex(
                                        manifoldElements,
                                        manifoldElements.IndexOf(manifoldElement),
                                        manifoldElement,
                                        maniElemType
                                        )) {
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }

                

                t.Commit();
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                MessageBox.Show(elapsedTime.ToString());
            }
        }
    }
}
