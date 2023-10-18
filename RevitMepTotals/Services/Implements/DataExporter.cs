using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using DevExpress.Spreadsheet;

using dosymep.SimpleServices;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements {
    internal class DataExporter : IDataExporter {
        private readonly ICopyNameProvider _copyNameProvider;
        private readonly IMessageBoxService _messageBoxService;

        public DataExporter(ICopyNameProvider copyNameProvider, IMessageBoxService messageBoxService) {
            _copyNameProvider = copyNameProvider ?? throw new ArgumentNullException(nameof(copyNameProvider));
            _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        }


        public void ExportData(DirectoryInfo exportDirectory, IList<IDocumentData> data) {
            if(exportDirectory is null) { throw new ArgumentNullException(nameof(exportDirectory)); }
            if(data is null) { throw new ArgumentNullException(nameof(data)); }
            if(data.Count == 0) { return; }
            CheckDistinctDocument(data);

            string path = CreateFileName(exportDirectory);
            // https://docs.devexpress.com/OfficeFileAPI/15072/spreadsheet-document-api/getting-started?v=21.2
            using(Workbook workbook = new Workbook()) {
                workbook.Unit = DevExpress.Office.DocumentUnit.Point;
                workbook.BeginUpdate();
                try {
                    for(int sheetIndex = 0; sheetIndex < data.Count; sheetIndex++) {
                        Worksheet worksheet = workbook.Worksheets[sheetIndex];
                        worksheet.Name = CleanSheetName(data[sheetIndex].Title); // приводим заголовок документа к корректному заголовку листа

                        worksheet.Rows[0][0].Value = "Воздуховоды";
                        int ductStart = 1;
                        IList<IDuctData> ductData = GetOrderedDuctData(data[sheetIndex]);
                        int ductEnd = WriteDuctData(worksheet, ductStart, ductData);

                        worksheet.Rows[ductEnd + 1][0].Value = "Трубы";
                        int pipeStart = ductEnd + 2;
                        IList<IPipeData> pipeData = GetOrderedPipeData(data[sheetIndex]);
                        int pipeEnd = WritePipeData(worksheet, pipeStart, pipeData);

                        worksheet.Rows[pipeEnd + 1][0].Value = "Изоляция воздуховодов";
                        int ductInsulationStart = pipeEnd + 2;
                        IList<IDuctInsulationData> ductInsulationData = GetOrderedDuctInsulationData(data[sheetIndex]);
                        int ductInsulationEnd = WriteDuctInsulationData(worksheet, ductInsulationStart, ductInsulationData);

                        worksheet.Rows[ductInsulationEnd + 1][0].Value = "Изоляция трубопроводов";
                        int pipeInsulationStart = ductInsulationEnd + 2;
                        IList<IPipeInsulationData> pipeInsulationData = GetOrderedPipeInsulationData(data[sheetIndex]);
                        WritePipeInsulationData(worksheet, pipeInsulationStart, pipeInsulationData);

                        worksheet.Columns.AutoFit(0, typeof(IDuctInsulationData).GetProperties().Count());
                        workbook.Worksheets.Add(); //добавляем следующий лист
                    }
                    workbook.Worksheets.RemoveAt(workbook.Worksheets.Count - 1); // удаляем последний пустой лист, который мы не заполняли
                } finally {
                    workbook.EndUpdate();
                }
                workbook.Calculate();
                workbook.SaveDocument(path, DocumentFormat.OpenXml);
            }
        }

        private void CheckDistinctDocument(ICollection<IDocumentData> data) {
            if(data.Distinct().Count() != data.Count) {
                ShowMessageBoxError("Нельзя выгрузить данные из документов с одинаковым названием!");
                throw new OperationCanceledException();
            }
        }

        private IList<IDuctData> GetOrderedDuctData(IDocumentData documentData) {
            return documentData.Ducts.OrderBy(d => d.TypeName).ToList();
        }

        private IList<IPipeData> GetOrderedPipeData(IDocumentData documentData) {
            return documentData.Pipes.OrderBy(pipe => pipe.TypeName).ToList();
        }

        private IList<IDuctInsulationData> GetOrderedDuctInsulationData(IDocumentData documentData) {
            return documentData.DuctInsulations.OrderBy(pipe => pipe.TypeName).ToList();
        }

        private IList<IPipeInsulationData> GetOrderedPipeInsulationData(IDocumentData documentData) {
            return documentData.PipeInsulations.OrderBy(pipe => pipe.TypeName).ToList();
        }

        /// <summary>
        /// Записывает данные по воздуховодам на заданный лист Excel, начиная с заданной строчки
        /// </summary>
        /// <param name="worksheet">Лист Excel для записи</param>
        /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
        /// <param name="ductData">Данные по воздуховодам</param>
        /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
        private int WriteDuctData(Worksheet worksheet, int startRow, IList<IDuctData> ductData) {
            int ductsCount = ductData.Count;
            int lastRow = startRow + ductsCount - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = ductData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = ductData[row - startRow].Name;
                worksheet.Rows[row][2].Value = ductData[row - startRow].Size;
                worksheet.Rows[row][3].Value = ductData[row - startRow].Length;
            }
            return lastRow;
        }

        /// <summary>
        /// Записывает данные по трубам на заданный лист Excel, начиная с заданной строчки
        /// </summary>
        /// <param name="worksheet">Лист Excel для записи</param>
        /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
        /// <param name="pipeData">Данные по трубам</param>
        /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
        private int WritePipeData(Worksheet worksheet, int startRow, IList<IPipeData> pipeData) {
            int pipesCount = pipeData.Count;
            int lastRow = startRow + pipesCount - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = pipeData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = pipeData[row - startRow].Name;
                worksheet.Rows[row][2].Value = pipeData[row - startRow].Size;
                worksheet.Rows[row][3].Value = pipeData[row - startRow].Length;
            }
            return lastRow;
        }

        /// <summary>
        /// Записывает данные по изоляции воздуховодов на заданный лист Excel, начиная с заданной строчки
        /// </summary>
        /// <param name="worksheet">Лист Excel для записи</param>
        /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
        /// <param name="ductInsulationData">Данные по изоляции воздуховодов</param>
        /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
        private int WriteDuctInsulationData(Worksheet worksheet, int startRow, IList<IDuctInsulationData> ductInsulationData) {
            int count = ductInsulationData.Count;
            int lastRow = startRow + count - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = ductInsulationData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = ductInsulationData[row - startRow].Name;
                worksheet.Rows[row][2].Value = ductInsulationData[row - startRow].DuctSize;
                worksheet.Rows[row][3].Value = ductInsulationData[row - startRow].Thickness;
                worksheet.Rows[row][4].Value = ductInsulationData[row - startRow].Length;
                worksheet.Rows[row][5].Value = ductInsulationData[row - startRow].Area;
            }
            return lastRow;
        }

        /// <summary>
        /// Записывает данные по изоляции труб на заданный лист Excel, начиная с заданной строчки
        /// </summary>
        /// <param name="worksheet">Лист Excel для записи</param>
        /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
        /// <param name="pipeInsulationData">Данные по изоляции труб</param>
        /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
        private int WritePipeInsulationData(Worksheet worksheet, int startRow, IList<IPipeInsulationData> pipeInsulationData) {
            int count = pipeInsulationData.Count;
            int lastRow = startRow + count - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = pipeInsulationData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = pipeInsulationData[row - startRow].Name;
                worksheet.Rows[row][2].Value = pipeInsulationData[row - startRow].PipeSize;
                worksheet.Rows[row][3].Value = pipeInsulationData[row - startRow].Thickness;
                worksheet.Rows[row][4].Value = pipeInsulationData[row - startRow].Length;
                worksheet.Rows[row][5].Value = pipeInsulationData[row - startRow].Area;
            }
            return lastRow;
        }


        /// <summary>
        /// Создает абсолютный путь для нового файла выгрузки
        /// </summary>
        /// <param name="exportDirectory">Директория для выгрузки</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string CreateFileName(DirectoryInfo exportDirectory) {
            if(exportDirectory is null) { throw new ArgumentNullException(nameof(exportDirectory)); }

            var suffix = DateTime.Now.ToString("yyyy-MM-dd");
            var fileExtension = ".xlsx";

            var docShortName = $"Выгрузка объемов ВИС-{suffix}";
            var docLongName = $"{exportDirectory.FullName}\\{docShortName}{fileExtension}";

            if(File.Exists(docLongName)) {
                string[] neighboringFilesNames = exportDirectory
                    .GetFiles()
                    .Select(f => Path.GetFileNameWithoutExtension(f.FullName))
                    .ToArray() ?? Array.Empty<string>();
                docShortName = _copyNameProvider.CreateCopyName(docShortName, neighboringFilesNames);
            }
            return $"{exportDirectory.FullName}\\{docShortName}{fileExtension}";
        }

        /// <summary>
        /// Корректирует название листа Excel в соответствии с правилами
        /// https://docs.devexpress.com/OfficeFileAPI/DevExpress.Spreadsheet.Worksheet.Name#remarks
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string CleanSheetName(string name) {
            var charsToRemove = new char[] { '\\', '/', '?', ':', '*', '[', ']' };
            string trimName = string.Concat(name.Trim().Take(31)).Trim();
            foreach(char charToRemove in charsToRemove) {
                trimName = trimName.Replace(charToRemove, '_');
            }
            return trimName;
        }

        private void ShowMessageBoxError(string error) {
            _messageBoxService.Show(error, "BIM",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK);
        }
    }
}
