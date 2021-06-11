using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

namespace RevitBatchPrint {
    internal class RevitPrint {
        private readonly Document _document;

        public RevitPrint(Document document) {
            _document = document;
        }

        public string PdfPrinterName { get; set; } = "PDFCreator";

        public string FilterParameterValue { get; set; }
        public IReadOnlyList<string> FilterParameterNames { get; set; } = new List<string>() { "Орг.ОбознчТома(Комплекта)", "ADSK_Комплект чертежей" };

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
            printManager.SelectNewPrintDriver(PdfPrinterName);

            List<ViewSheet> viewSheets = GetViewSheets()
                .Where(item => IsAllowPrintSheet(item))

                // Сортировка вызывает сомнения
                .OrderBy(item => Convert.ToInt32(Regex.Match(item.SheetNumber, @"\d+$").Groups[0].Value))
                .ToList();


            foreach(ViewSheet viewSheet in viewSheets) {
                try {
                    using(Transaction transaction = new Transaction(_document, "PrintSettings")) {
                        transaction.Start();
                        try {
                            var printSettings = GetPrintSettings(viewSheet);
                            if(printSettings.Format == null) {
                                continue;
                            }

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

            var pWidth = familyInstance.get_Parameter(BuiltInParameter.SHEET_WIDTH);
            var pHeight = familyInstance.get_Parameter(BuiltInParameter.SHEET_HEIGHT);

            if(int.TryParse(pWidth?.AsValueString(), out int width) && int.TryParse(pHeight?.AsValueString(), out int height)) {
                return new PrintSettings() { Format = Format.GetFormat(familyInstance) ?? Format.GetFormat(width, height), FormatOrientation = GetFormatOrientation(width, height) };
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
