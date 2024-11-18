using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;


namespace RevitMirroredElements.Models {
    internal class RevitRepository {

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public const string MirrorFilterName = "Проверка зеркальности";
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public ICollection<ElementId> SelectElementsOnView() {

            var uiDocument = UIApplication.ActiveUIDocument;

            var selectedReferences = uiDocument.Selection.PickObjects(
                Autodesk.Revit.UI.Selection.ObjectType.Element,
                "Выберите элементы");

            if(selectedReferences != null) {
                return selectedReferences
                    .Select(reference => reference.ElementId)
                    .ToList();
            }

            return new List<ElementId>();
        }
        public ICollection<ElementId> GetElementsIdsFromCategories(List<Category> selectedCategories, ElementScope selectedGroupType) {
            var elements = new List<ElementId>();

            foreach(var category in selectedCategories) {
                var collector = new FilteredElementCollector(Document)
                    .OfCategory(category.GetBuiltInCategory())
                    .WhereElementIsNotElementType();

                if(selectedGroupType == ElementScope.ActiveView) {
                    var viewId = Document.ActiveView.Id;

#if REVIT_2021_OR_LESS
                    var filteredElements = collector
                        .Where(item => item.OwnerViewId == Document.ActiveView.Id)
                        .ToList();

                    collector = new FilteredElementCollector(Document, filteredElements.Select(e => e.Id).ToList());
#else
                    collector = collector.WherePasses(new VisibleInViewFilter(Document, viewId));
#endif
                }

                elements.AddRange(collector.ToElementIds());
            }
            return elements;
        }
        public ICollection<Element> GetElements(ICollection<ElementId> elementIds) {
            var elements = new List<Element>();


            foreach(var id in elementIds) {
                Element element = Document.GetElement(id);

                if(element is FamilyInstance familyInstance) {
                    elements.Add(element);
                }
            }
            return elements;
        }
        public ICollection<ElementId> GetSelectedElementsIds() {
            return ActiveUIDocument.Selection.GetElementIds();
        }
        public ICollection<Category> GetCategories() {
            Categories categories = Document.Settings.Categories;

            var modelCategories = categories
                .Cast<Category>()
                .Where(c => c.CategoryType == CategoryType.Model)
                .ToList();

            return modelCategories;
        }
        private ElementId GetParameterIdByName(string paramName) {
            var paramElement = new FilteredElementCollector(Document)
                .OfClass(typeof(SharedParameterElement))
                .Cast<SharedParameterElement>()
                .FirstOrDefault(e => e.Name == paramName);
            return paramElement?.Id;
        }
        private OverrideGraphicSettings CreateMirrorCheckGraphicOverrides() {
            var overrideSettings = new OverrideGraphicSettings();
            overrideSettings.SetSurfaceTransparency(0);
            overrideSettings.SetHalftone(false);
            overrideSettings.SetProjectionLineColor(new Color(255, 0, 0));

            var solidLinePattern = new FilteredElementCollector(Document)
                .OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>()
                .FirstOrDefault(p => p.Name == "<Solid>");

            if(solidLinePattern != null) {
                overrideSettings.SetProjectionLinePatternId(solidLinePattern.Id);
            }

            overrideSettings.SetProjectionLineWeight(1);

            var solidFillPattern = new FilteredElementCollector(Document)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(p => p.GetFillPattern().IsSolidFill);

            if(solidFillPattern != null) {
                overrideSettings.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                overrideSettings.SetCutForegroundPatternId(solidFillPattern.Id);
            }

            overrideSettings.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));
            overrideSettings.SetCutForegroundPatternColor(new Color(255, 0, 0));

            return overrideSettings;
        }
        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }
        public void FilterOnTemporaryView(List<Element> elements) {
            View activeView = Document.ActiveView;
            using(Transaction transaction = StartTransaction("Настройка временного вида для зеркальности")) {

                activeView.EnableTemporaryViewPropertiesMode(activeView.Id);

                var collector = new FilteredElementCollector(Document).OfClass(typeof(ParameterFilterElement));
                ParameterFilterElement mirrorCheckFilter = null;

                foreach(ParameterFilterElement filter in collector) {
                    if(filter.Name == MirrorFilterName) {
                        mirrorCheckFilter = filter;
                        break;
                    }
                }

                if(mirrorCheckFilter == null) {
                    string paramName = SharedParamsConfig.Instance.ElementMirroring.Name;
                    ElementId paramId = GetParameterIdByName(paramName);
                    if(paramId == null) {
                        throw new InvalidOperationException($"Параметр '{paramName}' не найден в документе.");
                    }

                    var rule = new FilterIntegerRule(new ParameterValueProvider(paramId), new FilterNumericEquals(), 1);

                    var categoryIds = elements
                        .Select(element => element.Category?.Id)
                        .Where(id => id != null)
                        .Distinct()
                        .ToList();

                    mirrorCheckFilter = ParameterFilterElement.Create(Document, MirrorFilterName, categoryIds);
                    mirrorCheckFilter.SetElementFilter(new ElementParameterFilter(rule));
                }

                var overrideSettings = CreateMirrorCheckGraphicOverrides();
                if(!activeView.GetFilters().Contains(mirrorCheckFilter.Id)) {
                    activeView.AddFilter(mirrorCheckFilter.Id);
                }
                activeView.SetFilterOverrides(mirrorCheckFilter.Id, overrideSettings);
                activeView.SetFilterVisibility(mirrorCheckFilter.Id, true);

                transaction.Commit();
            }
        }
        public void SelectElementsOnMainView(List<Element> elements) {
            var elementIds = elements.Select(x => x.Id).ToList();
            ActiveUIDocument.Selection.SetElementIds(elementIds);
        }

    }
}
