using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitDeleteUnused.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public void DeleteSelectedCommand(List<Element> elements) {
            using(Transaction t = Document.StartTransaction("Удалить неиспользуемые")) {
                foreach(Element elementToDelete in elements) {
                    Document.Delete(elementToDelete.Id);
                }
                t.Commit();
            }
        }

        public void SetAll(List<ElementToDeleteViewModel> allLinks, bool value) {
            foreach(var element in allLinks) { element.IsChecked = value; }
        }

        public void InvertAll(List<ElementToDeleteViewModel> allLinks) {
            foreach(var element in allLinks) { element.IsChecked = !element.IsChecked; }
        }

        public List<ElementToDeleteViewModel> GetViewTemplates() {
            List<View> allViews = new FilteredElementCollector(Document)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .Cast<View>()
                .ToList();

            List<ElementId> usedViewTemplates = allViews
                .Where(view => view.ViewTemplateId.IsNotNull())
                .Select(view => view.ViewTemplateId)
                .Distinct()
                .ToList();

            List<ElementToDeleteViewModel> elementsToDelete = allViews
                .Where(view => view.IsTemplate)
                .Select(item => new ElementToDeleteViewModel(item.Name, item, usedViewTemplates.Contains(item.Id)))
                .Where(item => !item.IsUsed)
                .OrderBy(e => e.Name)
                .ToList();

            return elementsToDelete;
        }

        public List<ElementToDeleteViewModel> GetFilters() {
            List<ElementId> usedFilters = new FilteredElementCollector(Document)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(view => view.AreGraphicsOverridesAllowed())
                .SelectMany(view => view.GetFilters())
                .ToList();

            List<Element> allFilters = new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .ToList();

            List<ElementToDeleteViewModel> elementsToDelete = allFilters
                .Select(item => new ElementToDeleteViewModel(item.Name, item, usedFilters.Contains(item.Id)))
                .Where(item => !item.IsUsed)
                .OrderBy(e => e.Name)
                .ToList();

            return elementsToDelete;
        }
    }
}