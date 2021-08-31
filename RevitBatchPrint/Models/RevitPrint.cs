using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Comparators;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.Models {
    internal class RevitPrint {
        private readonly RevitRepository _revitRepository;

        public RevitPrint(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        /// <summary>
        /// Наименование принтера печати.
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// Наименование параметра альбома по которому будет фильтрация листов.
        /// </summary>
        public string FilterParamName { get; set; }

        /// <summary>
        /// Значение параметра альбома по которому будет фильтрация листов.
        /// </summary>
        public string FilterParamValue { get; set; }

        /// <summary>
        /// Список ошибок при выводе на печать.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        public void Execute() {
            PrintManager printManager = _revitRepository.Document.PrintManager;
            printManager.PrintToFile = true;
            printManager.PrintToFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.ChangeExtension(Path.GetFileName(_revitRepository.Document.PathName), ".pdf"));

            List<ViewSheet> viewSheets = _revitRepository.GetViewSheets(FilterParamName, FilterParamValue)
                .OrderBy(item => item, new ViewSheetComparer())
                .ToList();

            var printerSettings = new Models.Printing.PrintManager().GetPrinterSettings(PrinterName);

            foreach(ViewSheet viewSheet in viewSheets) {
                var printSettings = GetPrintSettings(viewSheet);
                bool hasFormatName = printerSettings.HasFormatName(printSettings.Format.Name);
                if(!hasFormatName) {
                    // создаем новый формат в Windows, если не был найден подходящий
                    printerSettings.AddFormat(printSettings.Format.Name, new System.Drawing.Size(printSettings.Format.Width, printSettings.Format.Height));

                    // перезагружаем в ревите принтер, чтобы появились изменения
                    printManager.SelectNewPrintDriver(PrinterName);
                }

                try {
                    using(Transaction transaction = new Transaction(_revitRepository.Document, "PrintSettings")) {
                        transaction.Start();

                        var paperSize = _revitRepository.GetPaperSizeByName(printSettings.Format.Name);

                        printManager.PrintSetup.CurrentPrintSetting = printManager.PrintSetup.InSession;
                        printManager.PrintSetup.CurrentPrintSetting.PrintParameters.PaperSize = paperSize;
                        printManager.PrintSetup.CurrentPrintSetting.PrintParameters.ZoomType = ZoomType.Zoom;
                        printManager.PrintSetup.CurrentPrintSetting.PrintParameters.Zoom = 100;
                        printManager.PrintSetup.CurrentPrintSetting.PrintParameters.PageOrientation = printSettings.FormatOrientation;

                        printManager.PrintSetup.SaveAs("SheetPrintSettings");

                        printManager.Apply();
                        printManager.SubmitPrint(viewSheet);
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

        private PrintSettings GetPrintSettings(ViewSheet viewSheet) {
            FamilyInstance familyInstance = _revitRepository.GetTitleBlock(viewSheet);
            if(familyInstance == null) {
                throw new InvalidOperationException("Не было обнаружено семейство основной надписи.");
            }

            return _revitRepository.GetPrintSettings(familyInstance);
        }
    }
}
