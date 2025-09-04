using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Comparators;

using RevitBatchPrint.Models;
using RevitBatchPrint.Services;

namespace RevitBatchPrint.Models {
    internal class RevitPrint : IRevitPrint {
        private readonly RevitRepository _revitRepository;
        private readonly IPrinterService _printerService;

        public RevitPrint(RevitRepository revitRepository, IPrinterService printerService) {
            _revitRepository = revitRepository;
            _printerService = printerService;
        }

        public void Execute(IReadOnlyCollection<SheetElement> sheets, PrintOptions printOptions) {
            IPrinterSettings printerSettings = _printerService.CreatePrinterSettings(printOptions.PrinterName);

            // Создаем все форматы листов
            CreateSheetFormats(printerSettings, sheets);

            foreach(SheetElement sheetElement in sheets) {
                // устанавливаем нужный принтер в настройках
                PrintManager printManager = SetPrintDevice(printOptions.PrinterName);
                printManager.PrintToFileName = _revitRepository.GetFileName(sheetElement.ViewSheet);
                
                // чтобы не появлялась ошибка (файл существует)
                if(File.Exists(printManager.PrintToFileName)) {
                    File.Delete(printManager.PrintToFileName);
                }

                var printSettings = printManager.PrintSetup.InSession;
                printManager.PrintSetup.CurrentPrintSetting = printSettings;

                using Transaction transaction = _revitRepository.Document.StartTransaction("PrintSettings");

                PrintSheetSettings printSheetSettings = sheetElement.PrintSheetSettings;
                
                PaperSize paperSize = printManager.PaperSizes
                    .OfType<PaperSize>()
                    .FirstOrDefault(item => item.Name.Equals(printSheetSettings.SheetFormat.Name));

                if(paperSize is null) {
                    throw new Exception("Не были найдены форматы листа принтера.");
                }

                PrintParameters printParameters = printSettings.PrintParameters;

                printParameters.PaperSize = paperSize;
                printParameters.PageOrientation = printSheetSettings.FormatOrientation;

                printOptions.Apply(printParameters);
                printManager.PrintSetup.SaveAs("PrintSettings");

                printManager.Apply();
                printManager.SubmitPrint(sheetElement.ViewSheet);

                transaction.RollBack();
            }

            // удаляем старые созданные форматы для принтера
            RemoveOldFormats(printerSettings);
        }

        private static void CreateSheetFormats(
            IPrinterSettings printerSettings,
            IReadOnlyCollection<SheetElement> sheets) {
            var sheetFormats = sheets
                .Select(item => item.PrintSheetSettings.SheetFormat)
                .Where(item => !printerSettings.HasFormat(item.Name));

            foreach(SheetFormat sheetFormat in sheetFormats) {
                printerSettings.AddFormat(
                    sheetFormat.Name,
                    new Size(sheetFormat.WidthMm, sheetFormat.HeightMm));
            }
        }

        private static void RemoveOldFormats(IPrinterSettings printerSettings) {
            IEnumerable<string> formats = printerSettings.EnumFormatNames()
                .Where(item => item.StartsWith(SheetFormat.CustomPrefix));

            foreach(string format in formats) {
                printerSettings.RemoveFormat(format);
            }
        }

        public PrintManager SetPrintDevice(string printerName) {
            PrintManager printManager = _revitRepository.Document.PrintManager;

            // После выбора принтера все настройки сбрасываются
            printManager.SelectNewPrintDriver(printerName);
            printManager.PrintToFile = true;
            printManager.PrintOrderReverse = false;

            // Должно быть установлено это значение,
            // если установлено другое значение,
            // имя файла устанавливается ревитом
            printManager.PrintRange = PrintRange.Current;
            printManager.PrintToFileName = _revitRepository.GetFileName(null);

            return printManager;
        }
    }
}
