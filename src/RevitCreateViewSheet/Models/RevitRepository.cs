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


        public string GetDefaultAlbum() {
            return ActiveUIDocument.GetSelectedElements()
                .OfType<ViewSheet>()
                .Select(item => (string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))
                .Distinct()
                .FirstOrDefault();
        }

        public List<string> GetAlbumsBlueprints() {
            return GetViewSheets()
                .Select(item => (string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))
                .Where(item => item?.EndsWith("BIM") == false)
                .OrderBy(item => item)
                .Distinct()
                .ToList();
        }

        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();
        }

        public List<FamilySymbol> GetTitleBlocks() {
            var category = Category.GetCategory(Document, BuiltInCategory.OST_TitleBlocks);
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .Where(item => item.Category.Id == category.Id)
                .ToList();
        }

        public int GetLastViewSheetIndex(string albumBlueprints) {
            ViewSheet viewSheet = GetViewSheets()
                .Where(item => ((string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))?.Equals(albumBlueprints) == true)
                .OrderBy(item => item, new ViewSheetComparer())
                .LastOrDefault();

            return GetViewSheetIndex(viewSheet) ?? 1;
        }

        public void RemoveElement(ElementId id) {
            Document.Delete(id);
        }

        public void CreateAnnotation(View view2D, FamilySymbol familySymbol, XYZ point) {
            Document.Create.NewFamilyInstance(point, familySymbol, view2D);
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
            var sheet = ViewSheet.Create(Document, sheetModel.TitleBlockSymbolId);
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
                    var symbol = Document.GetElement(sheetModel.TitleBlockSymbolId) as FamilySymbol;
                    (Document.GetElement(titleId) as FamilyInstance).Symbol = symbol;
                }
            }
            sheet.SetParamValue(SharedParamsConfig.Instance.StampSheetNumber, sheetModel.SheetNumber);
            sheet.SetParamValue(SharedParamsConfig.Instance.AlbumBlueprints, sheetModel.AlbumBlueprints);
            sheet.SetParamValue(BuiltInParameter.SHEET_NAME, sheetModel.Name);
            sheet.SetParamValue(BuiltInParameter.SHEET_NUMBER, $"{sheetModel.AlbumBlueprints}-{sheetModel.SheetNumber}");
            return sheet;
        }

        private int? GetViewSheetIndex(ViewSheet viewSheet) {
            string index = viewSheet?.SheetNumber.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return int.TryParse(index, out int result) ? result : (int?) null;
        }

        private void InitializeParameters(Application application, Document document) {
            var projectParameters = ProjectParameters.Create(application);
            projectParameters.SetupRevitParams(document,
                SharedParamsConfig.Instance.AlbumBlueprints,
                SharedParamsConfig.Instance.StampSheetNumber);
        }
    }
}
