using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Comparators;

using RevitBatchPrint.Models;
using RevitBatchPrint.Services;

namespace RevitBatchPrint.Models {
    internal class RevitPrint : IRevitPrint {
        private readonly RevitRepository _revitRepository;
        private readonly PrintManager _printManager;
        private readonly IPrinterService _printerService;

        public RevitPrint(RevitRepository revitRepository, PrintManager printManager, IPrinterService printerService) {
            _revitRepository = revitRepository;
            
            _printManager = printManager;
            _printerService = printerService;
        }

        public void Execute(IReadOnlyCollection<SheetElement> sheets, PrintOptions printOptions) {
            IPrinterSettings printerSettings = _printerService.CreatePrinterSettings(printOptions.PrinterName);

            // удаляем старые созданные форматы для принтера
            // из-за того что отправка на печать асинхронная
            // приходится созданные форматы оставлять
            RemoveOldFormats(printerSettings);

            foreach(SheetElement sheetElement in sheets) {
                PrintSheetSettings printSheetSettings = sheetElement.PrintSheetSettings;

                // создаем формат, если его не было
                CreateFormatIfNotExists(printerSettings, printSheetSettings);

                // перезагружаем в ревите принтер, чтобы появились изменения
                _revitRepository.ReloadPrintSettings(printOptions.PrinterName);

                using Transaction transaction = _revitRepository.Document.StartTransaction("PrintSettings");

                _printManager.PrintSetup.CurrentPrintSetting = _printManager.PrintSetup.InSession;

                PaperSize paperSize = _revitRepository.GetPaperSizeByName(printSheetSettings.SheetFormat.Name);
                if(paperSize is null) {
                    throw new Exception($"Не были найдены форматы листа принтера.");
                }

                PrintParameters printParameters = _printManager.PrintSetup.CurrentPrintSetting.PrintParameters;

                printParameters.PaperSize = paperSize;
                printParameters.PageOrientation = printSheetSettings.FormatOrientation;

                printOptions.SetupPrintParams(printParameters);

                _revitRepository.UpdatePrintFileName(sheetElement.ViewSheet);
                _printManager.PrintSetup.SaveAs("SheetPrintSettings");

                _printManager.Apply();
                _printManager.SubmitPrint(sheetElement.ViewSheet);

                transaction.RollBack();
            }
        }

        private static void CreateFormatIfNotExists(
            IPrinterSettings printerSettings,
            PrintSheetSettings printSheetSettings) {
            if(printerSettings.HasFormat(printSheetSettings.SheetFormat.Name)) {
                return;
            }

            // создаем новый формат в Windows, если не был найден подходящий
            printerSettings.AddFormat(printSheetSettings.SheetFormat.Name,
                new Size(printSheetSettings.SheetFormat.WidthMm, printSheetSettings.SheetFormat.HeightMm));
        }

        private static void RemoveOldFormats(IPrinterSettings printerSettings) {
            IEnumerable<string> formats = printerSettings.EnumFormatNames()
                .Where(item => item.StartsWith(SheetFormat.CustomPrefix));

            foreach(string format in formats) {
                printerSettings.RemoveFormat(format);
            }
        }
    }
}
