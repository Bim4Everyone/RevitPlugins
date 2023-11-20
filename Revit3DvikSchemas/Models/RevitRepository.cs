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


        public RevitRepository(UIApplication uiApplication, List<BuiltInCategory> systemAndFopCats, List<BuiltInCategory> fopNameCategoriesBI) {
            UIApplication = uiApplication;
            systemAndFopCats = new List<BuiltInCategory>() { BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_FlexDuctCurves, BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_DuctAccessory, BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment, BuiltInCategory.OST_DuctInsulations, BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures, BuiltInCategory.OST_Sprinklers, BuiltInCategory.OST_GenericModel
            };
            SystemAndFopCats = systemAndFopCats;

            fopNameCategoriesBI = getfopNameCategoriesBIC();
            FopNameCategoriesBI = fopNameCategoriesBI;

        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public List<BuiltInCategory> SystemAndFopCats { get; }

        public List<BuiltInCategory> FopNameCategoriesBI { get; }

        public List<BuiltInCategory> getfopNameCategoriesBIC() {
            if(Document.IsExistsSharedParam("ФОП_ВИС_Имя системы")) {
                (Definition Definition, Binding Binding) fopName = Document.GetSharedParamBinding("ФОП_ВИС_Имя системы");
                Binding parameterBinding = fopName.Binding;
                IEnumerable<Category> fopNameCategories = parameterBinding.GetCategories();
                List<BuiltInCategory> fopNameCategoriesBIC = new List<BuiltInCategory>();
                foreach(Category cat in fopNameCategories) {
                    BuiltInCategory catBIC = cat.GetBuiltInCategory();
                    fopNameCategoriesBIC.Add(catBIC);
                }
                return fopNameCategoriesBIC;
            } else {
                return null;
            }

        }


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
            if(Document.IsExistsSharedParam("ФОП_ВИС_Имя системы")) {
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

