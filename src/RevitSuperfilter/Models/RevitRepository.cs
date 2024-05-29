using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitSuperfilter.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public IList<Element> GetAllElements() {
            var collector1 = new FilteredElementCollector(_document)
                .WhereElementIsNotElementType();

            var collector2 = new FilteredElementCollector(_document)
                .WhereElementIsElementType();

            return Enumerable.Empty<Element>().Concat(collector1).Concat(collector2).ToList();
        }

        public IList<Element> GetElements() {
            return GetElements(_document.ActiveView);
        }

        public IList<Element> GetElements(View view) {
            var elements = new FilteredElementCollector(_document, view.Id)
                .ToElements();

            var elementTypes = elements
                .Select(item => item.GetTypeId())
                .Where(item => item != ElementId.InvalidElementId)
                .Select(item => _document.GetElement(item));

            return elements
                .Union(elementTypes)
                .Distinct(new ElementComparer())
                .ToList();
        }

        public IList<Element> GetElements(Category category) {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategoryId(category.Id)
                .ToElements();
        }

        public IList<Element> GetElements(Category category, View view) {
            return new FilteredElementCollector(_document, view.Id)
                .WhereElementIsNotElementType()
                .OfCategoryId(category.Id)
                .ToElements();
        }

        public IList<Element> GetSelectedElements() {
            var selectedElements = _uiDocument.Selection.GetElementIds()
                .Select(item => _document.GetElement(item))
                .ToList();

            var elementTypes = selectedElements
                .Select(item => item.GetTypeId())
                .Where(item => item != ElementId.InvalidElementId)
                .Select(item => _document.GetElement(item));

            return selectedElements
                .OfType<Group>()
                .SelectMany(item => GetElements(item))
                .Union(elementTypes)
                .Union(selectedElements)
                .Distinct(new ElementComparer())
                .ToList();
        }

        public IEnumerable<Element> GetElements(Group group) {
            var children = GetElements(group.GetMemberIds());
            var reqursive = children.OfType<Group>().SelectMany(item => GetElements(item));

            return children.Union(reqursive).Distinct(new ElementComparer());
        }

        public IList<Category> GetCategories() {
            return _document.Settings.Categories.OfType<Category>().Where(item => item.Parent == null).ToList();
        }

        public IList<Category> GetCategories(IEnumerable<Element> elements) {
            return elements.Select(item => item.Category).Distinct(new CategoryComparer()).ToList();
        }

        public void SetSelectedElements(IEnumerable<Element> elements) {
            _uiDocument.Selection.SetElementIds(elements.Select(item => item.Id).ToList());
        }

        public void SetSelectedElements(IEnumerable<ElementId> elements) {
            _uiDocument.Selection.SetElementIds(elements.ToList());
        }

        public void ShowElements(IEnumerable<Element> elements) {
            _uiDocument.ShowElements(elements.Select(item => item.Id).ToList());
        }

        public void ShowElements(IEnumerable<ElementId> elements) {
            _uiDocument.ShowElements(elements.ToList());
        }

        public Element GetElement(ElementId elementId) {
            return _document.GetElement(elementId);
        }

        public IEnumerable<Element> GetElements(IEnumerable<ElementId> elements) {
            return elements.Select(item => _document.GetElement(item));
        }

        public IList<ParameterElement> GetParameterElements() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ParameterElement))
                .OfType<ParameterElement>()
                .ToList();
        }
    }
}
