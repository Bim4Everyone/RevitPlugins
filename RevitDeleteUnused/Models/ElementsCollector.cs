using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDeleteUnused.Models {
    internal class ElementsCollector 
    {
        private readonly Document _doc;
        private readonly FilteredElementCollector fec;
        private readonly List<View> allViews;

        public ElementsCollector(Document doc) {
            _doc = doc;
            fec = new FilteredElementCollector(doc);
            allViews = fec.OfClass(typeof(View)).WhereElementIsNotElementType().Cast<View>().ToList();
        }

        public List<ElementToDelete> GetViewTemplates() {
            List<View> allViewTemplates = allViews.Where(view => view.IsTemplate).ToList();
            List<ElementId> usedViewTemplates = allViews.Where(view => view.ViewTemplateId.IntegerValue > 0)
                                                        .Select(view => view.ViewTemplateId)
                                                        .ToList();
            List<ElementToDelete> elementsToDelete = allViewTemplates.Select(item => new ElementToDelete(item.Name, item, usedViewTemplates.Contains(item.Id)))
                                                                         .Where(item => !item.IsUsed)
                                                                         .ToList();

            return elementsToDelete;
        }

        public List<ElementToDelete> GetFilters() {
            List<ElementId> usedFilters = allViews.Where(view => view.AreGraphicsOverridesAllowed())
                                                  .Select(view => view.GetFilters())
                                                  .SelectMany(item => item)
                                                  .ToList();
            FilteredElementCollector fecFilters = new FilteredElementCollector(_doc);
            List<Element> allFilters = fecFilters.OfClass(typeof(ParameterFilterElement)).ToElements().ToList();
            List<ElementToDelete> elementsToDelete = allFilters.Select(item => new ElementToDelete(item.Name, item, usedFilters.Contains(item.Id)))
                                                                           .Where(item => !item.IsUsed)
                                                                           .ToList();

            return elementsToDelete;
        }
    }
}
