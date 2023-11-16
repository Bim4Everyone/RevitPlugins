using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Revit;


using Revit3DvikSchemas.ViewModels;

namespace Revit3DvikSchemas.Models {

    internal class RevitRepository {


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;



        //public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<Element> GetCollection(BuiltInCategory category) {
            List<Element> col = (List<Element>) new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements();
            return col;
        }

        public string GetSystemFopName(Element system) {
            string fopName = "None";
            if(system.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
                MechanicalSystem ductSystemElement = system as MechanicalSystem;
                foreach(Element duct in ductSystemElement.DuctNetwork) {
                    if(duct.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
                        if(duct.LookupParameter("ФОП_ВИС_Имя системы").HasValue) {
                            return duct.LookupParameter("ФОП_ВИС_Имя системы").AsString();
                        }
                    }
                }

            }

            if(system.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                PipingSystem pipeSystemElement = system as PipingSystem;
                foreach(Element pipe in pipeSystemElement.PipingNetwork) {
                    if(pipe.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                        if(pipe.LookupParameter("ФОП_ВИС_Имя системы").HasValue) {
                            return pipe.LookupParameter("ФОП_ВИС_Имя системы").AsString();
                        }
                    }
                }
            }
            return fopName;

        }
        public List<HvacSystemViewModel> GetHVACSystems() {
            List<Element> ductSystems = GetCollection(BuiltInCategory.OST_DuctSystem);

            List<Element> pipeSystems = GetCollection(BuiltInCategory.OST_PipingSystem);

            List<Element> allSystems = ductSystems.Concat(pipeSystems).ToList();

            List<HvacSystemViewModel> newSystems = new List<HvacSystemViewModel>();

            foreach(Element system in allSystems) {
                var newSystem = new HvacSystemViewModel();
                newSystem.SystemElement = system;
                newSystem.SystemName = system.Name;
                newSystem.FopName = GetSystemFopName(system);
                newSystems.Add(newSystem);
            }

            return newSystems;

        }


    }


}

