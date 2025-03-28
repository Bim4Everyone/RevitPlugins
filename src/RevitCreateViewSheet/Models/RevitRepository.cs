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
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .ToArray();
        }

        public int GetLastViewSheetIndex(string albumBlueprints) {
            ViewSheet viewSheet = GetViewSheets()
                .Where(item => ((string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))?.Equals(albumBlueprints) == true)
                .OrderBy(item => item, new ViewSheetComparer())
                .LastOrDefault();

            return GetViewSheetIndex(viewSheet) ?? 1;
        }

        public void DeleteElement(ElementId id) {
            Document.Delete(id);
        }

        public void CreateAnnotation(View view2D, FamilySymbol familySymbol, XYZ point) {
            Document.Create.NewFamilyInstance(point, familySymbol, view2D);
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
            sheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, sheetModel.SheetNumber);
            sheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, sheetModel.AlbumBlueprint);
            sheet.SetParamValue(BuiltInParameter.SHEET_NAME, sheetModel.Name);
            sheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, $"{sheetModel.AlbumBlueprint}-{sheetModel.SheetNumber}");
            return sheet;
        }

        private int? GetViewSheetIndex(ViewSheet viewSheet) {
            string index = viewSheet?.SheetNumber.Split(['-'], StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return int.TryParse(index, out int result) ? result : null;
        }

        private void InitializeParameters(Application application, Document document) {
            var projectParameters = ProjectParameters.Create(application);
            projectParameters.SetupRevitParams(document,
                SharedParamsConfig.Instance.AlbumBlueprints,
                SharedParamsConfig.Instance.StampSheetNumber);
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
