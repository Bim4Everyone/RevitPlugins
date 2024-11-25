using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;


namespace RevitMirroredElements.Models {
    internal class RevitRepository {

        public const string MirrorFilterName = "Проверка зеркальности";

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        private ElementId GetParameterIdByName(string paramName) {
            var paramElement = new FilteredElementCollector(Document)
                .OfClass(typeof(SharedParameterElement))
                .Cast<SharedParameterElement>()
                .FirstOrDefault(e => e.Name == paramName);

            return paramElement?.Id ?? ElementId.InvalidElementId;
        }

        private OverrideGraphicSettings CreateMirrorCheckGraphicOverrides() {
            var overrideSettings = new OverrideGraphicSettings()
                .SetSurfaceTransparency(0)
                .SetHalftone(false)
                .SetProjectionLineColor(new Color(255, 0, 0))
                .SetProjectionLineWeight(1);

            SetLinePatterns(overrideSettings);
            SetFillPatterns(overrideSettings);

            return overrideSettings;
        }

        private void SetLinePatterns(OverrideGraphicSettings settings) {
            var solidLinePattern = new FilteredElementCollector(Document)
                .OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>()
                .FirstOrDefault(p => p.Name == "<Solid>");

            if(solidLinePattern != null) {
                settings.SetProjectionLinePatternId(solidLinePattern.Id);
            }
        }

        private void SetFillPatterns(OverrideGraphicSettings settings) {
            var solidFillPattern = new FilteredElementCollector(Document)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(p => p.GetFillPattern().IsSolidFill);

            if(solidFillPattern != null) {
                settings.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                settings.SetCutForegroundPatternId(solidFillPattern.Id);
            }

            settings.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));
            settings.SetCutForegroundPatternColor(new Color(255, 0, 0));
        }

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

        private void EnableTemporaryViewMode(View view) {
            view.EnableTemporaryViewPropertiesMode(view.Id);
        }

        private ParameterFilterElement GetOrCreateMirrorFilter(List<Element> elements) {
            var userName = Application.Username;
            string filterNameWithUser = $"{MirrorFilterName}_{userName}";

            var existingFilter = FindFilterByName(filterNameWithUser);
            var paramId = GetParameterIdByName(SharedParamsConfig.Instance.ElementMirroring.Name);

            if(paramId == ElementId.InvalidElementId) {
                throw new InvalidOperationException($"Параметр '{SharedParamsConfig.Instance.ElementMirroring.Name}' не найден.");
            }

            var rule = new FilterDoubleRule(new ParameterValueProvider(paramId), new FilterNumericEquals(), 1, 1e-6);
            var newCategoryIds = elements
                .Select(e => e.Category?.Id)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            if(existingFilter != null) {
                var existingCategoryIds = existingFilter.GetCategories().ToList();
                var combinedCategoryIds = existingCategoryIds
                    .Concat(newCategoryIds)
                    .Distinct()
                    .ToList();

                existingFilter.SetCategories(combinedCategoryIds);
                existingFilter.SetElementFilter(new ElementParameterFilter(rule));

                return existingFilter;
            } else {
                var newFilter = ParameterFilterElement.Create(Document, filterNameWithUser, newCategoryIds);
                newFilter.SetElementFilter(new ElementParameterFilter(rule));
                return newFilter;
            }
        }

        private ParameterFilterElement FindFilterByName(string filterName) {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .FirstOrDefault(f => f.Name == filterName);
        }

        private void ApplyFilterToView(View view, ParameterFilterElement filter, OverrideGraphicSettings settings) {
            if(!view.GetFilters().Contains(filter.Id)) {
                view.AddFilter(filter.Id);
            }
            view.SetFilterOverrides(filter.Id, settings);
            view.SetFilterVisibility(filter.Id, true);
        }

        public ICollection<ElementId> GetElementsIdsFromCategories(List<Category> selectedCategories, ElementScope scope) {
            return selectedCategories
                .SelectMany(category => GetElementsByCategory(category, scope))
                .ToList();
        }

        private IEnumerable<ElementId> GetElementsByCategory(Category category, ElementScope scope) {
            var collector = new FilteredElementCollector(Document)
                .OfCategory(category.GetBuiltInCategory())
                .WhereElementIsNotElementType();

            return scope == ElementScope.ActiveView
                ? FilterByActiveView(collector)
                : collector.ToElementIds();
        }

        private IEnumerable<ElementId> FilterByActiveView(FilteredElementCollector collector) {
            var viewId = Document.ActiveView.Id;

#if REVIT_2021_OR_LESS
    return collector
        .Where(item => item.OwnerViewId == viewId)
        .ToElementIds();
#else
            return collector.WherePasses(new VisibleInViewFilter(Document, viewId)).ToElementIds();
#endif
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
            var sharedParamElement = new FilteredElementCollector(Document)
                .OfClass(typeof(SharedParameterElement))
                .Cast<SharedParameterElement>()
                .FirstOrDefault(e => e.Name == SharedParamsConfig.Instance.ElementMirroring.Name);

            if(sharedParamElement == null) {
                throw new InvalidOperationException($"Параметр '{SharedParamsConfig.Instance.ElementMirroring.Name}' не найден.");
            }

            BindingMap bindingMap = Document.ParameterBindings;
            var bindings = bindingMap.ForwardIterator();
            List<Category> categories = new List<Category>();

            while(bindings.MoveNext()) {
                if(bindings.Key.GetElementId() == sharedParamElement.Id) {
                    if(bindings.Current is InstanceBinding instanceBinding) {
                        categories.AddRange(instanceBinding.Categories.Cast<Category>());
                    }
                }
            }
            return categories;
        }

        public void UpdateParams() {
            ProjectParameters projectParameters = ProjectParameters.Create(Application);
            projectParameters.SetupRevitParams(ActiveUIDocument.Document,
                SharedParamsConfig.Instance.ElementMirroring);
        }

        public ICollection<Category> GetCategoriesByElementIds(ICollection<ElementId> elementIds) {
            if(elementIds == null || !elementIds.Any()) {
                return new List<Category>();
            }
            var categories = new List<Category>();
            foreach(ElementId elementId in elementIds) {
                var category = Category.GetCategory(Document, elementId);
                categories.Add(category);
            }

            return categories;
        }

        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }

        public void FilterOnTemporaryView(List<Element> elements) {
            using(var transaction = StartTransaction("Настройка временного вида для зеркальности")) {
                var activeView = Document.ActiveView;
                EnableTemporaryViewMode(activeView);

                var mirrorCheckFilter = GetOrCreateMirrorFilter(elements);
                var overrideSettings = CreateMirrorCheckGraphicOverrides();

                ApplyFilterToView(activeView, mirrorCheckFilter, overrideSettings);
                transaction.Commit();
            }
        }

        public void SelectElementsOnMainView(List<Element> elements) {
            var elementsIds = new List<ElementId>();
            foreach(var element in elements) {
                var isMirrored = element.GetParamValue<double>(SharedParamsConfig.Instance.ElementMirroring.Name);
                if(isMirrored != 0) {
                    elementsIds.Add(element.Id);
                }
            }
            ActiveUIDocument.Selection.SetElementIds(elementsIds);
        }
    }
}
