using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDeleteUnused.Models {
    internal class ElementsCollector 
    {
        public static ObservableCollection<ElementToDelete> GetViewTemplates(Document doc) {
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            List<View> allViews = fec.OfClass(typeof(View)).Cast<View>().ToList();

            List<View> allViewTemplates = allViews.Where(view => view.IsTemplate).ToList();

            List<ElementId> usedViewTemplates = allViews.Where(view => view.ViewTemplateId.IntegerValue > 0)
                                                        .Select(view => view.ViewTemplateId)
                                                        .ToList();

            List<ElementToDelete> elementsToDeleteList = allViewTemplates.Select(item => new ElementToDelete(item.Name, item, usedViewTemplates.Contains(item.Id)))
                                                                         .Where(item => !item.IsUsed)
                                                                         .ToList();

            ObservableCollection<ElementToDelete> elementsToDelete = new ObservableCollection<ElementToDelete>(elementsToDeleteList);

            return elementsToDelete;
        }

        public static ObservableCollection<ElementToDelete> GetFilters(Document doc) {
            FilteredElementCollector fecViews = new FilteredElementCollector(doc);
            List<View> allViews = fecViews.OfClass(typeof(View)).WhereElementIsNotElementType().ToElements().Cast<View>().ToList();

            List<ElementId> usedFilters = allViews.Where(view => view.AreGraphicsOverridesAllowed())
                                                  .Select(view => view.GetFilters())
                                                  .SelectMany(item => item)
                                                  .ToList();

            FilteredElementCollector fecFilters = new FilteredElementCollector(doc);
            ICollection<Element> allFilters = fecFilters.OfClass(typeof(ParameterFilterElement)).ToElements();

            List<ElementToDelete> elementsToDeleteList = allFilters.Select(item => new ElementToDelete(item.Name, item, usedFilters.Contains(item.Id)))
                                                                           .Where(item => !item.IsUsed)
                                                                           .ToList();

            ObservableCollection<ElementToDelete> elementsToDelete = new ObservableCollection<ElementToDelete>(elementsToDeleteList);

            return elementsToDelete;
        }
    }
}
