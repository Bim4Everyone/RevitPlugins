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
            SpecConfiguration specConfiguration = new SpecConfiguration(Document);
            using(Transaction t = Document.StartTransaction("Обновление спецификации")) {

                foreach(Element element in Elements) {
                    new ElementParamDefaultFiller(
                        fromParamName: specConfiguration.OriginalParamNameMark,
                        toParamName: specConfiguration.TargetNameMark,
                        specConfiguration: specConfiguration).Fill(element);
                    new ElementParamDefaultFiller( 
                        fromParamName: specConfiguration.OriginalParamNameCode,
                        toParamName: specConfiguration.TargetNameCode,
                        specConfiguration: specConfiguration).Fill(element);
                    new ElementParamDefaultFiller(
                        fromParamName: specConfiguration.OriginalParamNameCreator,
                        toParamName: specConfiguration.TargetNameCreator, 
                        specConfiguration: specConfiguration).Fill(element);
                    new ElementParamUnitFiller(
                        fromParamName: specConfiguration.OriginalParamNameUnit,
                        toParamName: specConfiguration.TargetNameUnit,
                        specConfiguration: specConfiguration).Fill(element);
                    new ElementParamNumberFiller(
                        fromParamName: "Skip",
                        toParamName: specConfiguration.TargetNameNumber,
                        specConfiguration: specConfiguration,
                        document: Document).Fill(element);
                    new ElementParamNameFiller(
                        fromParamName: specConfiguration.OriginalParamNameName,
                        toParamName: specConfiguration.TargetNameName,
                        specConfiguration: specConfiguration).Fill(element);

                }
                t.Commit();
            }
        }
    }
}
