using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Revit3DvikSchemas.ViewModels;

namespace Revit3DvikSchemas.Models {
    internal class ViewMaker {


        public void CreateFilter() {
        }

        public string MakeViewName(HvacSystemViewModel system, bool useFopNames) {
            string name = null;

            if(useFopNames) {
                name = name + "_" + system.FopName;

            } else {
                name = name + "_" + system.SystemName;
            }

            return name;

        }

        public string MakeGroupViewName(List<HvacSystemViewModel> systems, bool useFopNames) {
            string name = null;

            foreach(HvacSystemViewModel system in systems) {
                if(useFopNames) {
                    name = name + "_" + system.FopName;

                } else {
                    TaskDialog.Show("Test", system.SystemName);
                    name = name + "_" + system.SystemName;
                }
            }
            return name;
        }

        public string IsNameUniqOrMakeNew(string name, List<Element> views) {
            string newName = name;
            foreach(Element view in views) {
                if(view.Name == name) {
                    newName = name + "_" + "Копия";

                }
            }
            return newName;
        }

        public void CopyViewCreateFilters(RevitRepository _revitRepository, List<HvacSystemViewModel> systems, bool useFopNames, bool combineViews) {
            RevitRepository revitRepository = _revitRepository;
            List<Element> viewCol = revitRepository.GetCollection(BuiltInCategory.OST_Views);

            string groupName = null;

            if(combineViews) {
                groupName = MakeGroupViewName(systems, useFopNames);

            }




            View activeView = _revitRepository.ActiveUIDocument.ActiveView;

            if(combineViews) {
                if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
                    ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
                    View newView = _revitRepository.Document.GetElement(newViewId) as View;
                    string newName = IsNameUniqOrMakeNew(groupName, viewCol);
                    newView.Name = newName;
                }
            } else {
                foreach(HvacSystemViewModel system in systems) {
                    string newName = MakeViewName(system, useFopNames);
                    if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
                        ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
                        View newView = _revitRepository.Document.GetElement(newViewId) as View;
                        newName = IsNameUniqOrMakeNew(newName, viewCol);
                        newView.Name = newName;
                    }
                }


            }

            //foreach(HvacSystemViewModel system in systems) {
            //    soloName = MakeViewName(system, useFopNames);
            //    if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
            //        ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
            //        View newView = _revitRepository.Document.GetElement(newViewId) as View;
            //    }
            //    }
            //if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
            //    ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
            //    View newView = _revitRepository.Document.GetElement(newViewId) as View;
            //    //newView.Name = "_Test";



            //   test = IsNameUniqOrMakeNew("_Test", viewCol);
            //}

        }
    }
}

