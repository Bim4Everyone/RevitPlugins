

using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Revit3DvikSchemas.ViewModels;

namespace Revit3DvikSchemas.Models {

    internal class ViewMaker {
        private RevitRepository _revitRepository;
        private bool combineViews;
        private bool useFopNames;


        public ViewMaker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public List<ElementFilter> CreateElementFilter(string systemName) {
            List<ElementFilter> elementFilterList = new List<ElementFilter>();
            ElementId nameId;
            if(useFopNames) {
                nameId = _revitRepository.Document.GetProjectParam("ФОП_ВИС_Имя системы").Id;
                ;
            } else {
                nameId = new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            }


#if REVIT_2022_OR_LESS
            ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName, true));
#else
            ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName));
#endif
            elementFilterList.Add(filterRule);
            return elementFilterList;
        }

        public ParameterFilterElement CreateFilter(string systemName) {
            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_DuctCurves));
            categories.Add(new ElementId(BuiltInCategory.OST_DuctAccessory));


            List<ElementFilter> elementFilterList = CreateElementFilter(systemName);
            LogicalAndFilter andFilter = new LogicalAndFilter(elementFilterList);

            ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(_revitRepository.Document, "Example view filter", categories, andFilter);

            return parameterFilterElement;
        }


        public void CreateSelectedCommand(List<HvacSystemViewModel> list, bool _useFopNames, bool _combineViews) {
            combineViews = _combineViews;
            useFopNames = _useFopNames;
            List<HvacSystemViewModel> systems = new List<HvacSystemViewModel>();
            foreach(HvacSystemViewModel system in list) {
                if(system.IsChecked) {
                    systems.Add(system);
                }
            }

            Transaction t = _revitRepository.Document.StartTransaction("Создать схемы");

            CopyViewCreateFilters(systems);

            t.Commit();
        }

        private void CopyViewCreateFilters(List<HvacSystemViewModel> systems) {

            RevitRepository revitRepository = _revitRepository;

            List<Element> viewCol = revitRepository.GetCollection(BuiltInCategory.OST_Views);

            FilteredElementCollector collector = new FilteredElementCollector(_revitRepository.Document);

            List<Element> filterCol = (List<Element>) collector.OfClass(typeof(ParameterFilterElement)).ToElements();


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
                        string uniqViewName = IsNameUniqOrMakeNew(newName, viewCol);

                        newView.Name = newName;

                        string filterName = "B4E_" + newName;
                        filterName = IsNameUniqOrMakeNew(filterName, filterCol);

                        ParameterFilterElement parameterFilterElement = CreateFilter(newName);
                    }
                }


            }

        }


        private static string IsNameUniqOrMakeNew(string name, List<Element> elements) {
            //bool isNameUniq = false;
            string newName = name;
            foreach(Element element in elements) {
                if(element.Name == name) {
                    newName = name + "_" + "Копия";
                }
            }
            return newName;
        }

        private static string MakeGroupViewName(List<HvacSystemViewModel> systems, bool useFopNames) {
            string name = null;

            foreach(HvacSystemViewModel system in systems) {
                if(useFopNames) {
                    name = name + "_" + system.FopName;

                } else {

                    name = name + "_" + system.SystemName;
                }
            }
            return name;
        }

        private static string MakeViewName(HvacSystemViewModel system, bool useFopNames) {
            string name = null;

            if(useFopNames) {
                name = name + "_" + system.FopName;

            } else {
                name = name + "_" + system.SystemName;
            }

            return name;

        }
    }
}

