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
using System.Xml.Linq;



namespace RevitMechanicalSpecification.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

        }
        //убрать
        public List<Element> Elements { get; set; }
        //убрать
        public List<MechanicalSystem> MechanicalSystems { get; set; }

        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;

        private List<ElementParamFiller> _fillers; 
        


        public void ExecuteSpecificationRefresh() {
            CollectionFactory collector = new CollectionFactory(Document);
            Elements = collector.GetMechanicalElements();
            MechanicalSystems = collector.GetMechanicalSystemColl();
            SpecConfiguration specConfiguration = new SpecConfiguration(Document.ProjectInformation);

            _fillers = new List<ElementParamFiller>() 
            {
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
                Document)
        };


            using(Transaction t = Document.StartTransaction("Обновление спецификации")) {

                foreach(Element element in Elements) {
                    foreach(ElementParamFiller filler in _fillers) {
                        filler.Fill(element);
                    }
                }
                t.Commit();
            }
        }
    }
}
