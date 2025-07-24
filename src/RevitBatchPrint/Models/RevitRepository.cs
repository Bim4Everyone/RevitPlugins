using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using InvalidOperationException = Autodesk.Revit.Exceptions.InvalidOperationException;

namespace RevitBatchPrint.Models {
    internal class RevitRepository {
        private readonly PrintManager _printManager;
        private readonly UIApplication _uiApplication;
        private readonly ILocalizationService _localizationService;

        public RevitRepository(
            PrintManager printManager, 
            UIApplication uiApplication, 
            ILocalizationService localizationService) {
            _printManager = printManager;
            _uiApplication = uiApplication;
            _localizationService = localizationService;
        }

        public Document Document => _uiApplication.ActiveUIDocument.Document;
        
        public void ReloadPrintSettings(string printerName) {
            // После выбора принтера все настройки сбрасываются
            _printManager.SelectNewPrintDriver(printerName);
            _printManager.PrintToFile = true;
            _printManager.PrintOrderReverse = false;

            // Должно быть установлено это значение,
            // если установлено другое значение,
            // имя файла устанавливается ревитом
            _printManager.PrintRange = PrintRange.Current;

            _printManager.Apply();
            UpdatePrintFileName(null);
        }
        
        public void UpdatePrintFileName(ViewSheet viewSheet) {
            string documentFileName = string.IsNullOrEmpty(Document.Title) 
                ? _localizationService.GetLocalizedString("Print.DefaultFileName") 
                : Document.Title;

            string viewName = ReplaceInvalidChars(viewSheet?.SheetNumber);
            string fileName = string.IsNullOrEmpty(viewName)
                ? documentFileName
                : $"{documentFileName} ({viewName} - {ReplaceInvalidChars(viewSheet?.Name)})";

            _printManager.PrintToFileName =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName + ".pdf");

            if(File.Exists(_printManager.PrintToFileName)) {
                File.Delete(_printManager.PrintToFileName);
            }
        }
        
        public List<string> GetAlbumParamNames() {
            var categoryId = new ElementId(BuiltInCategory.OST_Sheets);
            return Document.GetParameterBindings()
                .Where(item => item.Binding is InstanceBinding)
                .Where(item =>
                    ((InstanceBinding) item.Binding).Categories
                    .OfType<Category>()
                    .Any(category => category.Id == categoryId))
#if REVIT_2021_OR_LESS
                .Where(item => item.Definition.ParameterType == ParameterType.Text)
#else
                .Where(item => item.Definition.GetDataType() == SpecTypeId.String.Text)
#endif
                .Select(item => item.Definition.Name)
                .OrderBy(item => item)
                .Distinct()
                .ToList();
        }
        
        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .Where(item => item.CanBePrinted)
                .Where(item => item.IsTemplate == false)
                .Where(item => item.ViewType == ViewType.DrawingSheet)
                .ToList();
        }

        public List<View> GetViewsWithoutCrop(ViewSheet viewSheet) {
            return viewSheet.GetAllPlacedViews()
                .Select(item => Document.GetElement(item))
                .OfType<View>()
                .Where(item => item.IsExistsParam(BuiltInParameter.VIEWER_CROP_REGION))
                .Where(item => !item.CropBoxActive)
                .ToList();
        }
        
        public PrintSheetSettings GetPrintSettings(ViewSheet viewSheet) {
            FamilyInstance familyInstance = GetTitleBlock(viewSheet);

            if(familyInstance is null) {
                throw new System.InvalidOperationException(
                    _localizationService.GetLocalizedString("ViewSheet.NotFoundTitleBlock", viewSheet.Name));
            }

            double sheetWidth = (double) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_WIDTH);
            double sheetHeight = (double) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_HEIGHT);

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
        
        public PaperSize GetPaperSizeByName(string formatName) {
            if(string.IsNullOrEmpty(formatName)) {
                throw new ArgumentException($"'{nameof(formatName)}' cannot be null or empty.", nameof(formatName));
            }

            return _printManager.PaperSizes.OfType<PaperSize>().FirstOrDefault(item => item.Name.Equals(formatName));
        }
        
        private FamilyInstance GetTitleBlock(ViewSheet viewSheet) {
            if(viewSheet is null) {
                throw new ArgumentNullException(nameof(viewSheet));
            }

            return (FamilyInstance) new FilteredElementCollector(Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .FirstOrDefault();
        }
        
        private static PageOrientationType GetFormatOrientation(int width, int height) {
            return width > height ? PageOrientationType.Landscape : PageOrientationType.Portrait;
        }
        
        private static string ReplaceInvalidChars(string filename) {
            return string.IsNullOrEmpty(filename)
                ? null
                : string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
