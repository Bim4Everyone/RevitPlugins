using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

        }

        public List<Element> Elements { get; set; }

        public List<MechanicalSystem> MechanicalSystems { get; set; }

        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument =>  UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;



        public void ExecuteSpecificationRefresh() 
        {
            DefaultCollector collector = new DefaultCollector(Document);
            Elements = collector.GetDefElementColls();
            MechanicalSystems = collector.GetDefSystemColl();
            SpecConfiguration specConfiguration = new SpecConfiguration(Document.ProjectInformation);
        }
    }
}
