using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Models {
    internal class ViewFactory {
        private readonly Document _document;
        private readonly UIDocument _uiDoc;
        private readonly ElementId _criterionId;
        private readonly ElementId _fopCriterionId;
        private readonly bool? _useFopNames;
        private readonly bool? _combineViews;
        public ViewFactory(Document document, UIDocument uIDocument, bool? useFopNames, bool? combineViews) {
            _document = document;
            _uiDoc = uIDocument;
            _criterionId = new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            _fopCriterionId = _document.GetSharedParam(AxonometryConfig.FopVisSystemName).Id;
            _useFopNames = useFopNames;
            _combineViews = combineViews;
        }

        //Копирует виды для каждого элемента выделенных систем, или поодиночно, или создавая один вид для всех выделенных.
        public void CreateViewsBySelectedSystems(List<HvacSystemViewModel> systemList) {

            if(_combineViews == true) {
                CopyCombinedViews(systemList);
            } else {
                foreach(var hvacSystem in systemList) {
                    CopySingleView(hvacSystem);
                }
            }
        }

        //Создает фильтр для применения к видам
        public ParameterFilterElement CreateFilter(string filterName, List<string> systemNameList) {
            List<ElementId> categories = AxonometryConfig.SystemCategories
                .Select(category => new ElementId(category))
                .ToList();

            //создаем лист из фильтров по именам систем
            List<ElementFilter> elementFilterList = CreateFilterRules(systemNameList);

            return ParameterFilterElement.Create(
                _document, filterName, categories, new LogicalAndFilter(elementFilterList));
        }

        //Создает правила фильтрации для применени в фильтре
        public List<ElementFilter> CreateFilterRules(List<string> systemNameList) {
            ElementId parameter = _criterionId;
            if(_useFopNames == true) {
                parameter = _fopCriterionId;
            }

            List<ElementFilter> elementFilterList = new List<ElementFilter>();
            foreach(string systemName in systemNameList) {
#if REVIT_2022_OR_LESS
                ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(parameter, systemName, true));
#else
            ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(parameter, systemName));
#endif
                elementFilterList.Add(filterRule);
            }
            return elementFilterList;
        }

        //Возвращает уникальное имя, если в проекте уже есть такие имена - добавляет "копия" и счетчик
        private string GetUniqName(string name, List<Element> elements) {
            string baseName = name;
            int counter = 1;

            while(elements.Any(view => view.Name == name)) {
                name = $"{baseName}_копия{counter}";
                counter++;
            }
            return name;
        }

        //Создает один вид для всех выделенных систем и применяет фильтр
        private void CopyCombinedViews(List<HvacSystemViewModel> systemList) {
            List<Element> views = _document.GetCollection(BuiltInCategory.OST_Views);

            List<string> nameList = (bool) _useFopNames
                ? systemList.Select(x => x.FopName).ToList()
                : systemList.Select(x => x.SystemName).ToList();

            string viewName = GetUniqName(string.Join(" ,", nameList), views);

            List<Element> filters = _document.GetParameterFilterElements();
            string filterName = GetUniqName("B4E_" + viewName, filters);

            ElementId newViewId = _uiDoc.ActiveView.Duplicate(ViewDuplicateOption.WithDetailing);
            View newView = _document.GetElement(newViewId) as View;
            newView.Name = viewName;

            ParameterFilterElement filter = CreateFilter(filterName, nameList);
            newView.AddFilter(filter.Id);
            newView.SetFilterVisibility(filter.Id, false);
        }

        //Копирует одиночный вид и применяет к нему фильтры
        private void CopySingleView(HvacSystemViewModel hvacSystem) {
            List<Element> views = _document.GetCollection(BuiltInCategory.OST_Views);
            string viewName = GetUniqName((bool) _useFopNames ? 
                hvacSystem.FopName : hvacSystem.SystemName, views);

            List<Element> filters = _document.GetParameterFilterElements();
            string filterName = GetUniqName("B4E_" + viewName, filters);

            ElementId newViewId = _uiDoc.ActiveView.Duplicate(ViewDuplicateOption.WithDetailing);
            View newView = _document.GetElement(newViewId) as View;
            newView.Name = viewName;

            ParameterFilterElement filter = CreateFilter(filterName, new List<string> {
                (bool) _useFopNames ? hvacSystem.FopName : hvacSystem.SystemName });
            newView.AddFilter(filter.Id);
            newView.SetFilterVisibility(filter.Id, false);
        }
    }
}

