using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMechanicalSpecification.Models.Classes;
using dosymep.Revit;
using RevitMechanicalSpecification.Models.Fillers;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties;



namespace RevitMechanicalSpecification.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

        }

        public List<Element> Elements { get; set; }

        public List<MechanicalSystem> MechanicalSystems { get; set; }

        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;



        public void ExecuteSpecificationRefresh() {
            DefaultCollector collector = new DefaultCollector(Document);
            Elements = collector.GetDefElementColls();
            MechanicalSystems = collector.GetDefSystemColl();
            SpecConfiguration specConfiguration = new SpecConfiguration(Document.ProjectInformation);
            using(Transaction t = Document.StartTransaction("Обновление спецификации")) {

                foreach(Element element in Elements) {
                    if(element.Category.IsId(BuiltInCategory.OST_DuctFitting)) {

                        DuctElementsCalculator calculator = new DuctElementsCalculator();
                        calculator.GetFittingArea(element);
                    }

                    new ElementParamDefaultFiller(
                        fromParamName: specConfiguration.ParamNameMark,
                        toParamName: specConfiguration.TargetNameMark).Fill(element);
                    new ElementParamDefaultFiller( 
                        fromParamName: specConfiguration.ParamNameCode,
                        toParamName: specConfiguration.TargetNameCode).Fill(element);
                    new ElementParamDefaultFiller(
                        fromParamName: specConfiguration.ParamNameCreator,
                        toParamName: specConfiguration.TargetNameCreator).Fill(element);
                    new ElementParamUnitFiller(
                        fromParamName: specConfiguration.ParamNameUnit,
                        toParamName: specConfiguration.TargetNameUnit,
                        isSpecifyDuctFittings: specConfiguration.IsSpecifyDuctFittings).Fill(element);
                }
                t.Commit();
            }
        }
    }
}
