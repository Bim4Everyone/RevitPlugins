using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitCreatingFiltersByValues.Models;
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
    public List<FilterElement> AllFilterElementsInView => Document.ActiveView.GetFilters()
        .Select(id => Document.GetElement(id) as FilterElement)
        .ToList();

    public List<string> AllFilterElementNames => new FilteredElementCollector(Document)
            .OfClass(typeof(ParameterFilterElement))
            .Select(p => p.Name)
            .ToList();

    /// <summary>
    /// Получает штриховку в виде сплошной заливки
    /// </summary>
    public FillPatternElement SolidFillPattern => FillPatternElement.GetFillPatternElementByName(Document, FillPatternTarget.Drafting, "<Сплошная заливка>");


    /// <summary>
    /// Получает все элементы, видимые на виде
    /// Пытается выполнить получение при помощи экспорта элементов на виде, в этом случае можно получить в т.ч. элементы из связей
    /// В случае сбоя, получение элементов происходит только из текущего файла Revit стандартным FilteredElementCollector
    /// </summary>
    /// <returns></returns>
    public List<Element> GetElementsInView() {
        List<Element> elementsInView;
        try {
            var filterElemsByExportService = new FilterElemsByExportService(Document);
            elementsInView = filterElemsByExportService.GetElements();
        } catch(Exception) {
            elementsInView = new FilteredElementCollector(Document, Document.ActiveView.Id)
                                    .WhereElementIsNotElementType()
                                    .ToElements()
                                    .ToList();
        }
        return elementsInView;
    }



    /// <summary>
    /// Находит объекты штриховок по именам, указанным пользователем
    /// </summary>
    public ObservableCollection<PatternsHelper> GetPatternsByNames(List<string> patternNames) {

        ObservableCollection<PatternsHelper> patterns = [];
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
        List<FilterElement> filters = AllFilterElementsInView;
        string checkString = string.Format("${0}/", UserName);

        foreach(FilterElement filter in filters) {
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

        ObservableCollection<CategoryElements> categoryElements = [];
        foreach(var elem in GetElementsInView()) {
            if(elem.Category is null) { continue; }

            var catOfElem = elem.Category;
            string elemCategoryName = catOfElem.Name;
            var elemCategoryId = catOfElem.Id;

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
                categoryElements.Add(new CategoryElements(catOfElem, elemCategoryId, checkFlag, [elem]));
            }
        }
        return categoryElements;
    }
}
