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

namespace RevitCreateViewSheet.Models {
    public class RevitRepository {

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

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

        public ICollection<FamilySymbol> GetTitleBlocks() {
            var titleBlocks = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .ToArray();
            if(titleBlocks.Length == 0) {
                throw new InvalidOperationException("В проект не загружено ни одно семейство основной надписи.");
            }
            return titleBlocks;
        }

        public ICollection<ElementType> GetViewPortTypes() {
            Viewport viewport = new FilteredElementCollector(Document)
                .OfClass(typeof(Viewport))
                .FirstElement() as Viewport;
            string errorMsg = "Не удалось получить список типов видовых экранов. " +
                "Создайте видовой экран на каком-либо листе вручную и перезапустите плагин.";
            ICollection<ElementId> viewportTypeIds;
            if(viewport is not null) {
                viewportTypeIds = viewport.GetValidTypes();
            } else {
                try {
                    using(var transaction = Document.StartTransaction("temp")) {
                        var titleBlockId = GetTitleBlocks().First().Id;
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

        public void DeleteElement(ElementId id) {
            Document.Delete(id);
        }

        public void CreateAnnotation(View view2D, FamilySymbol familySymbol, XYZ point) {
            if(!familySymbol.IsActive) {
                familySymbol.Activate();
            }
            Document.Create.NewFamilyInstance(point, familySymbol, view2D);
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

        public ICollection<ViewSchedule> GetNotPlacedSchedules() {
            var placedSchedulesIds = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ScheduleSheetInstance))
                .OfType<ScheduleSheetInstance>()
                .Select(s => s.ScheduleId)
                .ToHashSet();
            return new FilteredElementCollector(Document)
                .Excluding(placedSchedulesIds)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ViewSchedule))
                .OfType<ViewSchedule>()
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
            return GetViewSheets()
                .Select(s => new SheetModel(s, GetTitleBlockSymbol(s)))
                .ToArray();
        }

        internal ScheduleSheetInstance CreateSchedule(ElementId viewSheetId, ElementId scheduleViewId, XYZ point) {
            return ScheduleSheetInstance.Create(Document, viewSheetId, scheduleViewId, point);
        }

        internal Viewport CreateViewPort(ElementId viewSheetId, ElementId viewId, ElementId viewportTypeId, XYZ point) {
            var viewport = Viewport.Create(Document, viewSheetId, viewId, point);
            var view = Document.GetElement(viewId) as View;
            view.CropBoxActive = true;
            view.CropBoxVisible = true;
            return UpdateViewPort(viewport, viewportTypeId);
        }

        internal Viewport UpdateViewPort(Viewport viewport, ElementId viewportTypeId) {
            viewport.SetParamValue(BuiltInParameter.ELEM_TYPE_PARAM, viewportTypeId);
            return viewport;
        }

        internal ViewSheet CreateViewSheet(SheetModel sheetModel) {
            var sheet = ViewSheet.Create(Document, sheetModel.TitleBlockSymbol.Id);
            return UpdateViewSheet(sheet, sheetModel, false);
        }

        internal ViewSheet UpdateViewSheet(ViewSheet sheet, SheetModel sheetModel) {
            return UpdateViewSheet(sheet, sheetModel, true);
        }

        private ViewSheet UpdateViewSheet(ViewSheet sheet, SheetModel sheetModel, bool updateTitleBlock) {
            if(updateTitleBlock) {
                var titleId = sheet.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks))
                    .FirstOrDefault();
                if(titleId.IsNotNull()) {
                    var symbol = Document.GetElement(sheetModel.TitleBlockSymbol.Id) as FamilySymbol;
                    (Document.GetElement(titleId) as FamilyInstance).Symbol = symbol;
                }
            }
            sheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, sheetModel.SheetCustomNumber);
            sheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, sheetModel.AlbumBlueprint);
            sheet.SetParamValue(BuiltInParameter.SHEET_NAME, sheetModel.Name);
            sheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, sheetModel.SheetNumber);
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

        private FamilySymbol GetTitleBlockSymbol(ViewSheet viewSheet) {
            var titleBlock = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementOwnerViewFilter(viewSheet.Id))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .FirstElement() as FamilyInstance;
            if(titleBlock is null) {
                throw new InvalidOperationException($"У листа {viewSheet.Name} отсутствует основная надпись");
            }
            return titleBlock.Symbol;
        }
    }
}
