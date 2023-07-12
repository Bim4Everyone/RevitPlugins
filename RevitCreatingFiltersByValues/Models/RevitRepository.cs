using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitCreatingFiltersByValues.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public string UserName => Application.Username;


        /// <summary>
        /// Возвращает перечень ElementId категорий, которые имеют параметры для фильтрации
        /// </summary>
        public List<ElementId> FilterableCategories => ParameterFilterUtilities.GetAllFilterableCategories().ToList();


        /// <summary>
        /// Получает все чертежные штриховки, имеющиеся в проекте
        /// </summary>
        public List<FillPatternElement> AllDraftingPatterns => new FilteredElementCollector(Document)
                .OfClass(typeof(FillPatternElement))
                .OfType<FillPatternElement>()
                .Where(item => item.GetFillPattern().Target == FillPatternTarget.Drafting)
                .ToList();
        
        /// <summary>
        /// Получает все фильтры, имеющиеся в проекте
        /// </summary>
        public List<ParameterFilterElement> AllFilterElements => new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .OfType<ParameterFilterElement>()
                .ToList();

        /// <summary>
        /// Получает все фильтры, имеющиеся на виде
        /// </summary>
        public List<ParameterFilterElement> AllFilterElementsInView => Document.ActiveView.GetFilters()
            .Select(id => Document.GetElement(id) as ParameterFilterElement)
            .ToList();

        public List<string> AllFilterElementNames => new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .Select(p => p.Name)
                .ToList();

        /// <summary>
        /// Получает штриховку в виде сплошной заливки
        /// </summary>
        public FillPatternElement SolidFillPattern => FillPatternElement.GetFillPatternElementByName(Document, FillPatternTarget.Drafting, "<Сплошная заливка>");


        public List<Element> GetElementsInView() {
            
            // Сначала получаем элементы на виде из текущего проекта
            List<Element> elementsInView = new FilteredElementCollector(Document, Document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            // Получаем экземпляры связей, видимые на виде
            List<RevitLinkInstance> links = new FilteredElementCollector(Document, Document.ActiveView.Id)
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();

            // Получаем и добавляем элементы из связей, видимые на виде
            try {
                foreach(RevitLinkInstance link in links) {
                    elementsInView.AddRange(new FilteredElementCollector(link.GetLinkDocument(), Document.ActiveView.Id)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .ToList());
                }
            } catch(Exception) {}

            return elementsInView;
        }




        /// <summary>
        /// Находит объекты штриховок по именам, указаным пользователем
        /// </summary>
        public ObservableCollection<PatternsHelper> GetPatternsByNames(List<string> patternNames) {

            ObservableCollection<PatternsHelper> patterns= new ObservableCollection<PatternsHelper>();
            List<FillPatternElement> allDraftingPatterns = AllDraftingPatterns;

            foreach(string patternName in patternNames) {
                foreach(FillPatternElement pattern in allDraftingPatterns) {
                    if(patternName.Equals(pattern.Name)) {
                        patterns.Add(new PatternsHelper(pattern, allDraftingPatterns));
                    }
                }
            }


            return patterns;
        }



        /// <summary>
        /// Удаляет все пользовательские временные виды (напр., "$divin_n_...")
        /// </summary>
        public void DeleteTempFiltersInView() {
            List<ParameterFilterElement> filters = AllFilterElementsInView;
            string checkString = string.Format("${0}/", UserName);

            foreach(ParameterFilterElement filter in filters) {
                if(filter.Name.Contains(checkString)) {
                    try {
                        Document.ActiveView.RemoveFilter(filter.Id);
                    } catch(Exception) { }
                }
            }
        }



        /// <summary>
        /// Получает категории, представленные на виде + элементы в словаре по ним
        /// </summary>
        public ObservableCollection<CategoryElements> GetCategoriesInView(bool checkFlag) {

            ObservableCollection<CategoryElements> categoryElements = new ObservableCollection<CategoryElements>();

            foreach(Element elem in GetElementsInView()) {
                if(elem.Category is null) { continue; }

                Category catOfElem = elem.Category;
                string elemCategoryName = catOfElem.Name;
                ElementId elemCategoryId = catOfElem.Id;

                // Отсеиваем категории, которые не имеют параметров фильтрации
                if(!FilterableCategories.Contains(elemCategoryId)) { continue; }

                // Добавляем в словарь элементы, разбивая по группам по ключу
                bool flag = false;
                foreach(CategoryElements categoryElement in categoryElements) {

                    if(categoryElement.CategoryName.Equals(elemCategoryName)) {
                        categoryElement.ElementsInView.Add(elem);
                        flag = true;
                        break;
                    }
                }

                if(flag is false) {
                    categoryElements.Add(new CategoryElements(catOfElem, elemCategoryId, checkFlag, new List<Element>() { elem }));
                }
            }

            return categoryElements;
        }

    }
}