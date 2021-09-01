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

        public Document Document => _revitRepository.Document;
        public PrintManager PrintManager => _revitRepository.PrintManager;
        public PrintParameters PrintParameters => PrintManager.PrintSetup.CurrentPrintSetting.PrintParameters;

        public Printing.PrinterSettings PrinterSettings { get; set; }

        public void Execute(Action<PrintParameters> setupPrintParams) {
            _revitRepository.ReloadPrintSettings(PrinterName);

            List<ViewSheet> viewSheets = _revitRepository.GetViewSheets(FilterParamName, FilterParamValue)
                .OrderBy(item => item, new ViewSheetComparer())
                .ToList();

            foreach(ViewSheet viewSheet in viewSheets) {
                var printSettings = GetPrintSettings(viewSheet);
                bool hasFormatName = PrinterSettings.HasFormatName(printSettings.Format.Name);
                if(!hasFormatName) {
                    // создаем новый формат в Windows, если не был найден подходящий
                    PrinterSettings.AddFormat(printSettings.Format.Name, new System.Drawing.Size(printSettings.Format.Width, printSettings.Format.Height));

                    // перезагружаем в ревите принтер, чтобы появились изменения
                    _revitRepository.ReloadPrintSettings(PrinterName);
                }

                try {
                    using(Transaction transaction = new Transaction(_revitRepository.Document, "PrintSettings")) {
                        transaction.Start();
                        
                        PrintManager.PrintSetup.CurrentPrintSetting = PrintManager.PrintSetup.InSession;
                        
                        PaperSize paperSize = _revitRepository.GetPaperSizeByName(printSettings.Format.Name);
                        PrintParameters.PaperSize = paperSize;
                        PrintParameters.PageOrientation = printSettings.FormatOrientation;

                        setupPrintParams(PrintParameters);

                        PrintManager.PrintSetup.SaveAs("SheetPrintSettings");

                        PrintManager.Apply();
                        PrintManager.SubmitPrint(viewSheet);
                    }
                } catch(Exception ex) {
                    Errors.Add($"{viewSheet.Name}: \"{ex.Message}\".");
                } finally {
                    if(!hasFormatName) {
                        PrinterSettings.RemoveFormat(printSettings.Format.Name);
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