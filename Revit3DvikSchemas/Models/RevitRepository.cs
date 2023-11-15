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
        private readonly ViewMaker _viewMaker;


        public RevitRepository(UIApplication uiApplication, ViewMaker viewMaker) {
            UIApplication = uiApplication;
            _viewMaker = viewMaker;
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
                        if(duct.LookupParameter("���_���_��� �������").HasValue) {
                            return duct.LookupParameter("���_���_��� �������").AsString();
                        }
                    }
                }

            }

            if(system.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                PipingSystem pipeSystemElement = system as PipingSystem;
                foreach(Element pipe in pipeSystemElement.PipingNetwork) {
                    if(pipe.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
                        if(pipe.LookupParameter("���_���_��� �������").HasValue) {
                            return pipe.LookupParameter("���_���_��� �������").AsString();
                        }
                    }
                }
            }
            return fopName;

        }

        public void CreateSelectedCommand(List<HvacSystemViewModel> list, bool useFopNames, bool combineViews) {
            List<HvacSystemViewModel> systems = new List<HvacSystemViewModel>();
            foreach(HvacSystemViewModel system in list) {
                if(system.IsChecked) {
                    systems.Add(system);
                }
            }
            Transaction t = Document.StartTransaction("������� ��������������");
            _viewMaker.CopyViewCreateFilters(this, systems, useFopNames, combineViews);

            t.Commit();

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
