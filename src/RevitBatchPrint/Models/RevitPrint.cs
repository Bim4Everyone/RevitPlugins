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

            // Получаем сообщение со всеми листами
            // у которых есть виды с отключенной подрезкой видов
            AddErrorCropView(viewSheets);

            var printErrors = new List<(ViewSheet ViewSheet, string Message)>();
            foreach(ViewSheet viewSheet in viewSheets) {
                var printSettings = GetPrintSettings(viewSheet);
                bool hasFormatName = PrinterSettings.HasFormatName(printSettings.Format.Name);
                if(!hasFormatName) {
                    // создаем новый формат в Windows, если не был найден подходящий
                    PrinterSettings.AddFormat(printSettings.Format.Name,
                        new System.Drawing.Size(printSettings.Format.Width, printSettings.Format.Height));

                    // перезагружаем в ревите принтер, чтобы появились изменения
                    _revitRepository.ReloadPrintSettings(PrinterName);
                }

                try {
                    using(Transaction transaction = new Transaction(_revitRepository.Document, "PrintSettings")) {
                        transaction.Start();

                        PrintManager.PrintSetup.CurrentPrintSetting = PrintManager.PrintSetup.InSession;

                        PaperSize paperSize = _revitRepository.GetPaperSizeByName(printSettings.Format.Name);
                        if(paperSize is null) {
                            throw new Exception($"Не были найдены форматы листа принтера.");
                        }

                        PrintParameters.PaperSize = paperSize;
                        PrintParameters.PageOrientation = printSettings.FormatOrientation;

                        setupPrintParams(PrintParameters);

                        _revitRepository.UpdatePrintFileName(viewSheet);
                        PrintManager.PrintSetup.SaveAs("SheetPrintSettings");

                        PrintManager.Apply();
                        PrintManager.SubmitPrint(viewSheet);
                    }
                } catch(Exception ex) {
                    printErrors.Add((viewSheet, ex.Message));
                } finally {
                    if(!hasFormatName) {
                        PrinterSettings.RemoveFormat(printSettings.Format.Name);
                    }
                }
            }

            AddExceptionError(printErrors);
        }

        private void AddExceptionError(List<(ViewSheet ViewSheet, string Message)> printErrors) {
            IEnumerable<string> messages = printErrors
                .GroupBy(item => item.Message)
                .OrderBy(item => item.Key)
                .Where(item => item.Any())
                .Select(item => GetMessage(item.Key, item.Select(viewSheet => viewSheet.ViewSheet)));

            if(messages.Any()) {
                Errors.Add(string.Join(Environment.NewLine, messages));
            }
        }

        private void AddErrorCropView(List<ViewSheet> viewSheets) {
            IEnumerable<string> messages = viewSheets
                .Select(item => (ViewSheet: item, ViewsWithoutCrop: _revitRepository.GetViewsWithoutCrop(item)))
                .Where(item => item.ViewsWithoutCrop.Count > 0)
                .Select(item => GetMessage(item.ViewSheet, item.ViewsWithoutCrop));

            if(messages.Any()) {
                Errors.Add("Листы у которые есть виды с отключенной подрезкой:" + Environment.NewLine +
                           string.Join(Environment.NewLine, messages));
            }
        }

        private string GetMessage(string message, IEnumerable<ViewSheet> viewSheets) {
            string separator = Environment.NewLine + "    - ";
            return $"   {message}:{separator}{string.Join(separator, viewSheets.Select(item => item.SheetNumber))}";
        }

        private string GetMessage(ViewSheet viewSheet, List<View> views) {
            string separator = Environment.NewLine + "        - ";
            return $"    {viewSheet.SheetNumber}:{separator}{string.Join(separator, views.Select(item => item.Name))}";
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