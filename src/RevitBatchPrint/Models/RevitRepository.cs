using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitBatchPrint.Models {
    internal class RevitRepository {
        private readonly UIApplication _uiApplication;
        private readonly IRevitParamFactory _revitParamFactory;
        private readonly ILocalizationService _localizationService;

        public RevitRepository(
            UIApplication uiApplication,
            IRevitParamFactory revitParamFactory,
            ILocalizationService localizationService) {
            _uiApplication = uiApplication;
            _revitParamFactory = revitParamFactory;
            _localizationService = localizationService;
        }

        public Document Document => _uiApplication.ActiveUIDocument.Document;

        public string GetFileName(ViewSheet viewSheet) {
            string documentFileName = string.IsNullOrEmpty(Document.Title)
                ? _localizationService.GetLocalizedString("Print.DefaultFileName")
                : Document.Title;

            string viewName = ReplaceInvalidChars(viewSheet?.SheetNumber);
            string fileName = string.IsNullOrEmpty(viewName)
                ? documentFileName
                : $"{documentFileName} ({viewName} - {ReplaceInvalidChars(viewSheet?.Name)})";

            string filePath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName + ".pdf");

            return filePath;
        }

        public List<RevitParam> GetAlbumParamNames() {
            return GetSheetParams()
                .OrderBy(item => item.Name)
                .ToList();
        }

        private IEnumerable<RevitParam> GetSheetParams() {
            var catParams = ParameterFilterUtilities
                .GetFilterableParametersInCommon(Document, new List<ElementId> { new(BuiltInCategory.OST_Sheets) });

            foreach(var elementId in catParams) {
                if(elementId.IsNotSystemId() && _revitParamFactory.CanCreate(Document, elementId)) {
                    yield return _revitParamFactory.Create(Document, elementId);
                }
            }
            
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_APPROVED_BY));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_CHECKED_BY));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_CURRENT_REVISION));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_DESIGNED_BY));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_DRAWN_BY));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_NUMBER));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_ISSUE_DATE));
            yield return _revitParamFactory.Create(Document, new ElementId(BuiltInParameter.SHEET_NAME));
        }

        public List<(ViewSheet ViewSheet, FamilyInstance TitleBlock, Viewport[] Viewports)> GetSheetsInfo() {
            Dictionary<ElementId, Viewport[]> viewPorts = GetViewPorts();
            Dictionary<ElementId, FamilyInstance> titleBlocks = GetFamilyInstances();

            // используем только основные надписи,
            // потому что они используются в печати и требуются их размеры
            IEnumerable<ElementId> sheetsIds = titleBlocks.Keys;

            return sheetsIds
                .Select(item => Document.GetElement(item))
                .OfType<ViewSheet>()
                .Where(item => item.CanBePrinted)
                .Where(item => item.IsTemplate == false)
                .Where(item => item.ViewType == ViewType.DrawingSheet)
                .Select(item => (item,
                    GetValueOrDefault(item.Id, titleBlocks),
                    GetValueOrDefault(item.Id, viewPorts)))
                .Where(item => item.Item2 is not null)
                .ToList();
        }

        private Dictionary<ElementId, Viewport[]> GetViewPorts() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(Viewport))
                .OfType<Viewport>()
                .Where(item => item.GetParamValueOrDefault<int?>(BuiltInParameter.VIEWER_CROP_REGION) == 0)
                .GroupBy(item => item.SheetId)
                .ToDictionary(item => item.Key, item => item.ToArray());
        }

        private Dictionary<ElementId, FamilyInstance> GetFamilyInstances() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfType<FamilyInstance>()
                .GroupBy(item => item.OwnerViewId)
                .ToDictionary(item => item.Key, item => item.FirstOrDefault());
        }

        public PrintSheetSettings GetPrintSettings(FamilyInstance familyInstance) {
            string widthParamName = _localizationService.GetLocalizedString("Settings.WidthParamName");
            string heightParamName = _localizationService.GetLocalizedString("Settings.HeightParamName");

            double sheetWidth = familyInstance.GetParamValueOrDefault<double?>(widthParamName)
                                ?? familyInstance.GetParamValueOrDefault<double>(BuiltInParameter.SHEET_WIDTH);

            double sheetHeight = familyInstance.GetParamValueOrDefault<double?>(heightParamName)
                                 ?? familyInstance.GetParamValueOrDefault<double>(BuiltInParameter.SHEET_HEIGHT);

#if REVIT_2020_OR_LESS
            sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth, DisplayUnitType.DUT_MILLIMETERS);
            sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight, DisplayUnitType.DUT_MILLIMETERS);
#else
            sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth, UnitTypeId.Millimeters);
            sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight, UnitTypeId.Millimeters);
#endif

            return new PrintSheetSettings() {
                SheetFormat = SheetFormat.GetFormat((int) Math.Round(sheetWidth), (int) Math.Round(sheetHeight)),
                FormatOrientation = GetFormatOrientation((int) Math.Round(sheetWidth), (int) Math.Round(sheetHeight))
            };
        }

        private static PageOrientationType GetFormatOrientation(int width, int height) {
            return width > height ? PageOrientationType.Landscape : PageOrientationType.Portrait;
        }

        private static string ReplaceInvalidChars(string filename) {
            return string.IsNullOrEmpty(filename)
                ? null
                : string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private T GetValueOrDefault<T>(ElementId key, Dictionary<ElementId, T> dictionary) {
            return dictionary.TryGetValue(key, out T value) ? value : default;
        }
    }
}
