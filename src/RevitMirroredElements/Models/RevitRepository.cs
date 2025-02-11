using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
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
            var solidNames = new List<string> { "<Solid>", "<Сплошная>" };
            var solidLinePattern = new FilteredElementCollector(Document)
                .OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>()
                .FirstOrDefault(p => solidNames.Contains(p.Name));

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

        public List<FamilyInstance> SelectElementsOnView() {
            var selectedReferences = ActiveUIDocument.Selection.PickObjects(
                Autodesk.Revit.UI.Selection.ObjectType.Element,
                "Выберите элементы");

            if(selectedReferences != null) {
                return selectedReferences
                    .Select(reference => ActiveUIDocument.Document.GetElement(reference.ElementId))
                    .OfType<FamilyInstance>()
                    .ToList();
            }


            return new List<FamilyInstance>();
        }

        private void EnableTemporaryViewMode(View view) {
            view.EnableTemporaryViewPropertiesMode(view.Id);// работает только при передаче view.Id если передать view.ViewTemplateId временный вид не создается 
        }

        private ParameterFilterElement GetOrCreateMirrorFilter(List<FamilyInstance> elements) {
            var userName = Application.Username;
            string filterNameWithUser = $"{MirrorFilterName}_{userName}";

            var existingFilter = FindFilterByName(filterNameWithUser);
            var sharedParam = SharedParamsConfig.Instance.ElementMirroring.GetRevitParamElement(Document);
            if(sharedParam.Id.IsNull()) {
                throw new InvalidOperationException($"Параметр '{SharedParamsConfig.Instance.ElementMirroring.Name}' не найден.");
            }

            var rule = new FilterDoubleRule(new ParameterValueProvider(sharedParam.Id), new FilterNumericEquals(), 1, 1e-6);
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

        public List<FamilyInstance> GetElementsFromCategories(List<Category> selectedCategories, ElementScope scope) {
            if(selectedCategories == null || !selectedCategories.Any()) {
                return new List<FamilyInstance>();
            }

            var categoryIds = selectedCategories
                .Where(c => c != null)
                .Select(c => c.Id)
                .ToList();

            var multiCategoryFilter = new ElementMulticategoryFilter(categoryIds);

            var collector = scope == ElementScope.ActiveView
               ? new FilteredElementCollector(Document, Document.ActiveView.Id)
                   .WherePasses(multiCategoryFilter)
                   .WhereElementIsNotElementType()
               : new FilteredElementCollector(Document)
                   .WherePasses(multiCategoryFilter)
                   .WhereElementIsNotElementType();


            return collector
                .OfType<FamilyInstance>()
                .ToList();
        }

        public List<FamilyInstance> GetSelectedElements() {
            var elements = new List<FamilyInstance>();
            var elementIds = ActiveUIDocument.Selection.GetElementIds();
            foreach(var id in elementIds) {
                Element element = Document.GetElement(id);

                if(element is FamilyInstance familyInstance) {
                    elements.Add(familyInstance);
                }
            }
            return elements;
        }

        public List<Category> GetCategories() {
            var revitParam = SharedParamsConfig.Instance.ElementMirroring;
            var categories = revitParam.GetParamBinding(Document).Binding.GetCategories();

            if(categories != null && categories.Any()) {
                return categories.ToList();
            }

            return new List<Category>();
        }

        public List<Category> GetSaveCategories(List<ElementId> categoriesIds) {
            if(categoriesIds == null || !categoriesIds.Any()) {
                return new List<Category>();
            }

            var categories = new List<Category>();

            foreach(var categoryId in categoriesIds) {
                var category = Category.GetCategory(Document, categoryId);
                if(category != null) {
                    categories.Add(category);
                }
            }

            return categories.Distinct().ToList();
        }

        public void UpdateParams() {
            ProjectParameters projectParameters = ProjectParameters.Create(Application);
            projectParameters.SetupRevitParams(ActiveUIDocument.Document,
                SharedParamsConfig.Instance.ElementMirroring);
        }

        public Transaction StartTransaction(string transactionName) {
            return Document.StartTransaction(transactionName);
        }

        public void FilterOnTemporaryView(List<FamilyInstance> elements) {
            using(var transaction = StartTransaction("Настройка временного вида для зеркальности")) {
                var activeView = Document.ActiveView;
                EnableTemporaryViewMode(activeView);

                var mirrorCheckFilter = GetOrCreateMirrorFilter(elements);
                var overrideSettings = CreateMirrorCheckGraphicOverrides();

                ApplyFilterToView(activeView, mirrorCheckFilter, overrideSettings);
                transaction.Commit();
            }
        }

        public void SelectElementsOnMainView(List<FamilyInstance> elements) {
            var elementsIds = new List<ElementId>();
            foreach(var element in elements) {
                var isMirrored = element.GetParamValue<double>(SharedParamsConfig.Instance.ElementMirroring);
                if(isMirrored != 0) {
                    elementsIds.Add(element.Id);
                }
            }
            ActiveUIDocument.Selection.SetElementIds(elementsIds);
        }
    }
}
