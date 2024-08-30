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
        private readonly CreationViewRules _creationViewRules;
        public ViewFactory(Document document, UIDocument uIDocument, CreationViewRules creationViewRules) {
            _document = document;
            _uiDoc = uIDocument;
            _creationViewRules = creationViewRules;
        }

        /// <summary>
        /// Копирует виды для каждого элемента выделенных систем, или поодиночно, или создавая один вид для всех выделенных.
        /// </summary>
        /// <param name="selectedSystemsList"></param>
        public void CreateViewsBySelectedSystems(List<HvacSystemViewModel> selectedSystemsList) {
            if(_creationViewRules.IsCombined) {
                CopyCombinedViews(selectedSystemsList);
            } else {
                foreach(var hvacSystem in selectedSystemsList) {
                    CopySingleView(hvacSystem);
                }
            }
        }

        /// <summary>
        /// Создает фильтр для применения к видам
        /// </summary>
        /// <returns></returns>
        public ParameterFilterElement CreateFilter(string filterName, List<string> systemNameList) {
            // создаем лист из фильтров по именам систем
            List<ElementFilter> elementFilterList = CreateFilterRules(systemNameList);

            return ParameterFilterElement.Create(
                _document, 
                filterName, 
                _creationViewRules.Categories, 
                new LogicalAndFilter(elementFilterList));
        }

        /// <summary>
        /// Создает правила фильтрации для применени в фильтре
        /// </summary>
        /// <returns></returns>
        public List<ElementFilter> CreateFilterRules(List<string> systemNameList) {
            List<ElementFilter> elementFilterList = new List<ElementFilter>();
            foreach(string systemName in systemNameList) {
#if REVIT_2022_OR_LESS
                ElementParameterFilter filterRule = 
                    new ElementParameterFilter(
                        ParameterFilterRuleFactory.CreateNotEqualsRule(_creationViewRules.FilterParameter, systemName, true));
#else
                ElementParameterFilter filterRule = 
                    new ElementParameterFilter(
                        ParameterFilterRuleFactory.CreateNotEqualsRule(_creationViewRules.FilterParameter, systemName));
#endif
                elementFilterList.Add(filterRule);
            }
            return elementFilterList;
        }

        /// <summary>
        /// Возвращает уникальное имя, если в проекте уже есть такие имена - добавляет "копия" и счетчик
        /// </summary>
        /// <returns></returns>
        private string GetUniqName(string name, List<Element> elements) {
            string baseName = name;
            int counter = 1;

            while(elements.Any(view => view.Name == name)) {
                name = $"{baseName}_копия{counter}";
                counter++;
            }
            return name;
        }

        /// <summary>
        /// Создает один вид для всех выделенных систем и применяет фильтр
        /// </summary>
        /// <returns></returns>
        private void CopyCombinedViews(List<HvacSystemViewModel> systemList) {
            List<Element> views = _document.GetElementsByCategory(BuiltInCategory.OST_Views);

            List<string> nameList = systemList.Select(x => _creationViewRules.GetSystemName(x)).ToList();

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

        /// <summary>
        /// Копирует одиночный вид и применяет к нему фильтры
        /// </summary>
        /// <returns></returns>
        private void CopySingleView(HvacSystemViewModel hvacSystem) {
            List<Element> views = _document.GetElementsByCategory(BuiltInCategory.OST_Views);
            string viewName = GetUniqName(_creationViewRules.GetSystemName(hvacSystem), views);

            List<Element> filters = _document.GetParameterFilterElements();
            string filterName = GetUniqName("B4E_" + viewName, filters);

            ElementId newViewId = _uiDoc.ActiveView.Duplicate(ViewDuplicateOption.WithDetailing);
            View newView = _document.GetElement(newViewId) as View;
            newView.Name = viewName;

            ParameterFilterElement filter =
                CreateFilter(filterName, 
                new List<string> {
                _creationViewRules.GetSystemName(hvacSystem)});
            newView.AddFilter(filter.Id);
            newView.SetFilterVisibility(filter.Id, false);
        }
    }
}

