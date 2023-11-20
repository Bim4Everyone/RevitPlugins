using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Revit3DvikSchemas.ViewModels;


namespace Revit3DvikSchemas.Models {

    internal class UniqName {
        public bool wasMet = true;
        public string name = null;
    }



    internal class ViewMaker {
        private RevitRepository _revitRepository;
        private bool combineViews;
        private bool useFopNames;


        public ViewMaker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        ElementId nameId = null;
        public List<ElementFilter> CreateElementFilter(List<string> systemNameList) {
            List<ElementFilter> elementFilterList = new List<ElementFilter>();
            if(useFopNames) {

                ParameterElement parameterElement = _revitRepository.Document.GetSharedParam("ФОП_ВИС_Имя системы");

                nameId = parameterElement.Id;
                ;
            } else {
                nameId = new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            }


            foreach(string systemName in systemNameList) {
#if REVIT_2022_OR_LESS
                ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName, true));
#else
            ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName));
#endif
                elementFilterList.Add(filterRule);



            }
            return elementFilterList;

        }

        public ParameterFilterElement CreateFilter(string filterName, List<string> systemNameList) {
            List<ElementId> categories = new List<ElementId>();
            List<BuiltInCategory> allCategories = new List<BuiltInCategory>() { BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves, BuiltInCategory.OST_FlexDuctCurves, BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_DuctAccessory, BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment, BuiltInCategory.OST_DuctInsulations, BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures, BuiltInCategory.OST_Sprinklers
            };

            foreach(BuiltInCategory category in allCategories) {
                categories.Add(new ElementId(category));
            }
            if(useFopNames) {
                categories.Add(new ElementId(BuiltInCategory.OST_GenericModel));
            }


            List<ElementFilter> elementFilterList = CreateElementFilter(systemNameList);



            LogicalAndFilter andFilter = new LogicalAndFilter(elementFilterList);

            ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(_revitRepository.Document, filterName, categories, andFilter);

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




            View activeView = _revitRepository.ActiveUIDocument.ActiveView;

            if(combineViews) {
                if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {

                    string groupName = MakeGroupViewName(systems, useFopNames);
                    string uniqViewName = IsNameUniqOrMakeNew(groupName, viewCol);
                    string filterName = "B4E_" + groupName;
                    filterName = IsNameUniqOrMakeNew(filterName, filterCol);

                    ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
                    View newView = _revitRepository.Document.GetElement(newViewId) as View;
                    newView.Name = uniqViewName;

                    List<string> systemNameList = new List<string>();
                    foreach(HvacSystemViewModel system in systems) {
                        if(useFopNames) {
                            systemNameList.Add(system.FopName);
                        } else {
                            systemNameList.Add(system.SystemName);
                        }

                    }
                    ParameterFilterElement parameterFilterElement = CreateFilter(filterName, systemNameList);
                    newView.AddFilter(parameterFilterElement.Id);
                    newView.SetFilterVisibility(parameterFilterElement.Id, false);
                }
            } else {
                foreach(HvacSystemViewModel system in systems) {
                    string systemName = MakeViewName(system, useFopNames);
                    if(activeView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
                        string uniqViewName = IsNameUniqOrMakeNew(systemName, viewCol);
                        List<string> systemNameList = new List<string>();
                        systemNameList.Add(systemName);
                        string filterName = "B4E_" + systemName;
                        filterName = IsNameUniqOrMakeNew(filterName, filterCol);

                        ElementId newViewId = activeView.Duplicate(ViewDuplicateOption.WithDetailing);
                        View newView = _revitRepository.Document.GetElement(newViewId) as View;
                        newView.Name = uniqViewName;
                        ParameterFilterElement parameterFilterElement = CreateFilter(filterName, systemNameList);

                        newView.AddFilter(parameterFilterElement.Id);
                        newView.SetFilterVisibility(parameterFilterElement.Id, false);
                    }
                }
            }
        }


        private static UniqName WasMet(List<Element> elements, UniqName uniqName) {
            foreach(Element element in elements) {
                if(element.Name == uniqName.name) {
                    uniqName.name = uniqName.name + "_" + "Копия";
                    return uniqName;
                }
            }

            uniqName.wasMet = false;
            return uniqName;
        }

        private static string IsNameUniqOrMakeNew(string name, List<Element> elements) {
            UniqName uniqName = new UniqName();
            uniqName.name = name;

            while(uniqName.wasMet == true) {
                uniqName = WasMet(elements,
                                  uniqName);
            }
            string newName = uniqName.name;
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
                name = system.FopName;

            } else {
                name = system.SystemName;
            }


            return name;

        }
    }
}

