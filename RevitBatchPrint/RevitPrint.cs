using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Comparators;

namespace RevitBatchPrint {
    internal class RevitPrint {
        private readonly Document _document;

        public RevitPrint(Document document) {
            _document = document;
        }

        public string PdfPrinterName { get; set; } = "PDFCreator";

        public string FilterParameterValue { get; set; }
        public IReadOnlyList<string> FilterParameterNames { get; set; } = new List<string>() { "Орг.ОбознчТома(Комплекта)", "Орг.КомплектЧертежей", "ADSK_Комплект чертежей" };

        public List<string> Errors { get; set; } = new List<string>();

        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(ViewSheet))
                .ToElements()
                .OfType<ViewSheet>()
                .Where(item => item.IsTemplate == false)
                .Where(item => item.CanBePrinted)
                .Where(item => item.ViewType == ViewType.DrawingSheet)
                .ToList();
        }

        public void Execute() {
            var printManager = _document.PrintManager;
            printManager.PrintToFile = true;
            printManager.PrintToFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(_document.PathName), ".pdf");

            List<ViewSheet> viewSheets = GetViewSheets()
                .Where(item => IsAllowPrintSheet(item))
                .OrderBy(item => item, new ViewSheetComparer())
                .ToList();

            var printerSettings = new Printing.PrintManager().GetPrinterSettings(PdfPrinterName);

            foreach(ViewSheet viewSheet in viewSheets) {
                var printSettings = GetPrintSettings(viewSheet);
                bool hasFormatName = printerSettings.HasFormatName(printSettings.Format.Name);
                if(!hasFormatName) {
                    // создаем новый формат в Windows, если не был найден подходящий
                    printerSettings.AddFormat(printSettings.Format.Name, new System.Drawing.Size(printSettings.Format.Width, printSettings.Format.Height));

                    // перезагружаем в ревите принтер, чтобы появились изменения
                    printManager.SelectNewPrintDriver(PdfPrinterName);
                }

                try {
                    using(Transaction transaction = new Transaction(_document, "PrintSettings")) {
                        transaction.Start();
                        
                        try {
                            var paperSize = GetPaperSizeByName(printManager, printSettings.Format.Name);

                            printManager.PrintSetup.CurrentPrintSetting = printManager.PrintSetup.InSession;
                            printManager.PrintSetup.CurrentPrintSetting.PrintParameters.PaperSize = paperSize;
                            printManager.PrintSetup.CurrentPrintSetting.PrintParameters.ZoomType = ZoomType.Zoom;
                            printManager.PrintSetup.CurrentPrintSetting.PrintParameters.Zoom = 100;
                            printManager.PrintSetup.CurrentPrintSetting.PrintParameters.PageOrientation = printSettings.FormatOrientation;

                            printManager.PrintSetup.SaveAs("SheetPrintSettings");

                            printManager.Apply();
                            printManager.SubmitPrint(viewSheet);
                        } finally {
                            transaction.RollBack();
                        }
                    }
                } catch(Exception ex) {
                    Errors.Add($"{viewSheet.Name}: \"{ex.Message}\".");
                } finally {
                    if(!hasFormatName) {
                        printerSettings.RemoveFormat(printSettings.Format.Name);
                    }
                }
            }
        }

        private bool IsAllowPrintSheet(ViewSheet viewSheet) {
            return FilterParameterNames.Any(item => viewSheet.LookupParameter(item)?.AsString()?.IndexOf(FilterParameterValue, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        private PrintSettings GetPrintSettings(ViewSheet viewSheet) {
            FamilyInstance familyInstance = new FilteredElementCollector(viewSheet.Document, viewSheet.Id)
                                               .OfClass(typeof(FamilyInstance))
                                               .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                               .OfType<FamilyInstance>()
                                               .FirstOrDefault();

            if(familyInstance == null) {
                throw new InvalidOperationException("Не было обнаружено семейство основной надписи.");
            }

            var sheetWidth = (double?) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_WIDTH);
            var sheetHeight = (double?) familyInstance.GetParamValueOrDefault(BuiltInParameter.SHEET_HEIGHT);

            if(sheetWidth.HasValue && sheetHeight.HasValue) {
                sheetWidth = UnitUtils.ConvertFromInternalUnits(sheetWidth.Value, DisplayUnitType.DUT_METERS);
                sheetHeight = UnitUtils.ConvertFromInternalUnits(sheetHeight.Value, DisplayUnitType.DUT_METERS);

                return new PrintSettings() {
                    Format = Format.GetFormat((int) sheetWidth, (int) sheetHeight),
                    FormatOrientation = GetFormatOrientation((int) sheetWidth, (int) sheetHeight)
                };
            }

            throw new InvalidOperationException("Не были обнаружены размеры основной надписи.");
        }

        private static PageOrientationType GetFormatOrientation(int width, int height) {
            return width > height ? PageOrientationType.Landscape : PageOrientationType.Portrait;
        }

        private PaperSize GetPaperSizeByName(PrintManager printManager, string formatName) {
            return printManager.PaperSizes.OfType<PaperSize>().First(item => item.Name.Equals(formatName));
        }
    }
}
