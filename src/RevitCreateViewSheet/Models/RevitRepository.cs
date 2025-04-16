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
using dosymep.Revit.Comparators;
using dosymep.SimpleServices;

using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.Models {
    internal class RevitRepository {
        private readonly EntitySaverProvider _entitySaverProvider;
        private readonly ILocalizationService _localizationService;

        public RevitRepository(
            UIApplication uiApplication,
            EntitySaverProvider entitySaverProvider,
            ILocalizationService localizationService) {

            UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
            _entitySaverProvider = entitySaverProvider ?? throw new ArgumentNullException(nameof(entitySaverProvider));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            InitializeParameters(Application, Document);
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public ICollection<string> GetAlbumsBlueprints() {
            return GetViewSheets()
                .Select(item => item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints, string.Empty))
                .Distinct()
                .OrderBy(item => item, new LogicalStringComparer())
                .ToArray();
        }

        public ICollection<FamilySymbol> GetTitleBlockSymbols() {
            var titleBlocks = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .ToArray();
            if(titleBlocks.Length == 0) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.TitleBlocksNotFound"));
            }
            return titleBlocks;
        }

        public ICollection<ElementType> GetViewPortTypes() {
            Viewport viewport = new FilteredElementCollector(Document)
                .OfClass(typeof(Viewport))
                .FirstElement() as Viewport;
            string errorMsg = _localizationService.GetLocalizedString("Errors.ViewPortTypesNotFound");
            ICollection<ElementId> viewportTypeIds;
            if(viewport is not null) {
                viewportTypeIds = viewport.GetValidTypes();
            } else {
                try {
                    using(var transaction = Document.StartTransaction("temp")) {
                        var titleBlockId = GetTitleBlockSymbols().First().Id;
                        ViewSheet viewSheet = ViewSheet.Create(Document, titleBlockId);
                        var view = new FilteredElementCollector(Document)
                            .OfClass(typeof(ViewPlan))
                            .OfType<ViewPlan>()
                            .First(v => Viewport.CanAddViewToSheet(Document, viewSheet.Id, v.Id));
                        // если вид пустой, то CanAddViewToSheet возвращает true, но вставить на лист его нельзя.
                        // Надо создать какой-то элемент на виде.
                        Document.Create.NewDetailCurve(view, Line.CreateBound(view.Origin, view.Origin + XYZ.BasisX));
                        viewport = Viewport.Create(Document, viewSheet.Id, view.Id, viewSheet.Origin);
                        viewportTypeIds = viewport?.GetValidTypes();
                        transaction.RollBack();
                    }
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    throw new InvalidOperationException(errorMsg);
                }
            }
            if(viewportTypeIds is null || viewportTypeIds.Count == 0) {
                throw new InvalidOperationException(errorMsg);
            }
            return viewportTypeIds.Select(Document.GetElement)
                .OfType<ElementType>()
                .ToArray();
        }

        public bool DeleteElement(ElementId id) {
            try {
                Document.Delete(id);
                return true;
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                return false;
            }
        }

        public FamilyInstance CreateAnnotation(View view2D, FamilySymbol familySymbol, XYZ point) {
            if(!familySymbol.IsActive) {
                familySymbol.Activate();
            }
            var instance = Document.Create.NewFamilyInstance(point, familySymbol, view2D);
            if(instance is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateAnnotation"));
            }
            return instance;
        }

        /// <summary>
        /// Возвращает все виды из активного документа, которые не являются листами 
        /// и которые могут быть размещены на листах в качестве видовых экранов.
        /// </summary>
        public ICollection<View> GetAllViewsForViewPorts() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .OfType<View>()
                .Where(v => v.CanBePrinted && IsViewTypeValidForViewPort(v.ViewType))
                .ToArray();
        }

        public ICollection<AnnotationSymbolType> GetAllAnnotationSymbols() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                .OfType<AnnotationSymbolType>()
                .ToArray();
        }

        public ICollection<ViewSchedule> GetAllSchedules() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSchedule))
                .OfType<ViewSchedule>()
                .Where(CanPlaceScheduleOnSheet)
                .ToArray();
        }

        /// <summary>
        /// Перечень типов видов, которые можно добавлять на листы в качестве видовых экранов.
        /// </summary>
        /// <param name="viewType">Тип вида</param>
        /// <returns>True, если плагин поддерживает вставку данного типа вида на лист в качестве видового экрана, 
        /// иначе false</returns>
        public bool IsViewTypeValidForViewPort(ViewType viewType) {
            return viewType switch {
                ViewType.FloorPlan => true,
                ViewType.CeilingPlan => true,
                ViewType.EngineeringPlan => true,
                ViewType.AreaPlan => true,
                ViewType.Section => true,
                ViewType.Elevation => true,
                ViewType.Detail => true,
                ViewType.ThreeD => true,
                ViewType.Rendering => true,
                ViewType.DraftingView => true,
                ViewType.Legend => true,
                _ => false
            };
        }

        internal ICollection<SheetModel> GetSheetModels() {
            ICollection<ViewSheet> viewSheets = GetViewSheets();
            var viewports = GetViewPorts()
                .GroupBy(v => v.SheetId)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var schedules = GetSchedules()
                .GroupBy(s => s.OwnerViewId)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var annotations = GetAnnotations()
                .GroupBy(a => a.OwnerViewId)
                .ToDictionary(g => g.Key, g => g.ToArray());
            var titleblocks = GetTitleBlockInstances()
                .GroupBy(a => a.GetParamValueOrDefault(BuiltInParameter.SHEET_NUMBER, string.Empty))
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.Symbol);

            return viewSheets.Select(sheet => new SheetModel(sheet,
                viewports.TryGetValue(sheet.Id, out Viewport[] viewportsArr)
                    ? viewportsArr : [],
                schedules.TryGetValue(sheet.Id, out ScheduleSheetInstance[] schedulesArr)
                    ? schedulesArr : [],
                annotations.TryGetValue(sheet.Id, out AnnotationSymbol[] annotationsArr)
                    ? annotationsArr : [],
                _entitySaverProvider.GetExistsEntitySaver(),
                titleblocks.TryGetValue(sheet.SheetNumber, out FamilySymbol titleBlock)
                    ? titleBlock : default))
                .ToArray();
        }

        internal ICollection<AnnotationSymbol> GetAnnotations() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                .OfType<AnnotationSymbol>()
                .Where(a => a.SuperComponent is null)
                .ToArray();
        }

        internal ICollection<ScheduleSheetInstance> GetSchedules() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ScheduleSheetInstance))
                .OfType<ScheduleSheetInstance>()
                .Where(s => CanPlaceScheduleOnSheet(Document.GetElement(s.ScheduleId) as ViewSchedule))
                .ToArray();
        }

        internal ICollection<Viewport> GetViewPorts() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Viewport))
                .OfType<Viewport>()
                .ToArray();
        }

        internal ScheduleSheetInstance CreateSchedule(ElementId viewSheetId, ElementId scheduleViewId, XYZ point) {
            var scheduleInstance = ScheduleSheetInstance.Create(Document, viewSheetId, scheduleViewId, point);
            if(scheduleInstance is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateSchedule"));
            }
            return scheduleInstance;
        }

        internal Viewport CreateViewPort(ViewPortModel viewPortModel) {
            if(!viewPortModel.Sheet.TryGetViewSheet(out var sheet)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateViewPortOnNotCreatedSheet"));
            }
            var viewport = Viewport.Create(Document, sheet.Id, viewPortModel.View.Id, viewPortModel.Location);
            if(viewport is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateViewPort"));
            }
            viewPortModel.View.CropBoxActive = true;
            viewPortModel.View.CropBoxVisible = true;
            viewPortModel.TrySetNewViewSheet(viewport);
            return UpdateViewPort(viewPortModel);
        }

        internal Viewport UpdateViewPort(ViewPortModel viewPortModel) {
            if(!viewPortModel.TryGetViewport(out var viewport)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotUpdateNotCreatetViewPort"));
            }
            if(viewPortModel.ViewPortType?.Id.IsNotNull() ?? false
                && viewPortModel.InitialViewPortType?.Id != viewPortModel.ViewPortType?.Id) {
                viewport.SetParamValue(BuiltInParameter.ELEM_TYPE_PARAM, viewPortModel.ViewPortType.Id);
            }
            return viewport;
        }

        internal ViewSheet CreateSheet(SheetModel sheetModel) {
            var sheet = ViewSheet.Create(Document, sheetModel.TitleBlockSymbol.Id);
            if(sheet is null) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotCreateSheet"));
            }
            sheetModel.TrySetNewViewSheet(sheet);
            return UpdateViewSheet(sheetModel, false);
        }

        internal ViewSheet UpdateViewSheet(SheetModel sheetModel) {
            return UpdateViewSheet(sheetModel, true);
        }

        private ViewSheet UpdateViewSheet(SheetModel sheetModel, bool updateTitleBlock) {
            if(!sheetModel.TryGetViewSheet(out var sheet)) {
                throw new InvalidOperationException(
                    _localizationService.GetLocalizedString("Errors.CannotUpdateNotCreatetSheet"));
            }
            if(updateTitleBlock
                && sheetModel.TitleBlockSymbol is not null
                && sheetModel.InitialTitleBlockSymbol?.Id != sheetModel.TitleBlockSymbol?.Id) {

                var titleId = sheet.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks))
                    .FirstOrDefault();
                var symbol = Document.GetElement(sheetModel.TitleBlockSymbol.Id) as FamilySymbol;
                if(titleId.IsNotNull()) {
                    (Document.GetElement(titleId) as FamilyInstance).Symbol = symbol;
                } else {
                    Document.Create.NewFamilyInstance(sheet.Origin, symbol, sheet);
                }
            }
            if(sheetModel.InitialSheetCustomNumber != sheetModel.SheetCustomNumber) {
                sheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, sheetModel.SheetCustomNumber);
            }
            if(sheetModel.InitialAlbumBlueprint != sheetModel.AlbumBlueprint) {
                sheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, sheetModel.AlbumBlueprint);
            }
            if(sheetModel.InitialName != sheetModel.Name) {
                sheet.SetParamValue(BuiltInParameter.SHEET_NAME, sheetModel.Name);
            }
            if(sheetModel.InitialSheetNumber != sheetModel.SheetNumber) {
                sheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, sheetModel.SheetNumber);
            }
            return sheet;
        }

        private void InitializeParameters(Application application, Document document) {
            var albumParam = SharedParamsConfig.Instance.AlbumBlueprints;
            var stampParam = SharedParamsConfig.Instance.StampSheetNumber;
            var projectParameters = ProjectParameters.Create(application);
            if(!document.IsExistsParam(albumParam)) {
                projectParameters.SetupRevitParams(document, albumParam);
            }
            if(!document.IsExistsParam(stampParam)) {
                projectParameters.SetupRevitParam(document, stampParam);
            }
        }

        private ICollection<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToArray();
        }

        /// <summary>
        /// Возвращает экземпляры основных надписей из документа
        /// </summary>
        private ICollection<FamilyInstance> GetTitleBlockInstances() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfType<FamilyInstance>()
                .ToArray();
        }

        private bool CanPlaceScheduleOnSheet(ViewSchedule schedule) {
            return schedule is not null
                && !schedule.IsInternalKeynoteSchedule
                && !schedule.IsTitleblockRevisionSchedule
                && !schedule.IsTemplate
                && !schedule.Definition.IsKeySchedule;
        }
    }
}
