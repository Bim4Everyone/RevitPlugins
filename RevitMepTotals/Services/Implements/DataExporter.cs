using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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


        public void ExportData(
            DirectoryInfo exportDirectory,
            IList<IDocumentData> dataForExport,
            out string exportError) {

            if(exportDirectory is null) { throw new ArgumentNullException(nameof(exportDirectory)); }
            if(dataForExport is null) { throw new ArgumentNullException(nameof(dataForExport)); }
            IList<IDocumentData> data = GetNotConflictedDocuments(dataForExport, out string documentErrors);
            exportError = documentErrors;

            if(data.Count == 0) {
                return;
            }
            string path = CreateFileName(exportDirectory);
            // https://docs.devexpress.com/OfficeFileAPI/15072/spreadsheet-document-api/getting-started?v=21.2
            using(Workbook workbook = new Workbook()) {
                workbook.Unit = DevExpress.Office.DocumentUnit.Point;
                workbook.BeginUpdate();
                try {
                    for(int sheetIndex = 0; sheetIndex < data.Count; sheetIndex++) {
                        Worksheet worksheet = workbook.Worksheets[sheetIndex];
                        worksheet.Name = CleanSheetName(data[sheetIndex].Title); // корректировка заголовка листа

                        int rowToWrite = 0;
                        rowToWrite = WriteDuctData(worksheet, rowToWrite, GetOrderedDuctData(data[sheetIndex]));

                        rowToWrite++;
                        rowToWrite = WritePipeData(worksheet, rowToWrite, GetOrderedPipeData(data[sheetIndex]));

                        rowToWrite++;
                        WritePipeInsulationData(worksheet, rowToWrite, GetOrderedPipeInsulationData(data[sheetIndex]));

                        worksheet.Columns
                            .AutoFit(0,
                                typeof(IPipeInsulationData)
                                .GetProperties()
                                .Count()
                            ); // интерфейс с самым большим количеством свойств
                        workbook.Worksheets.Add(); //добавляем следующий лист
                    }
                    workbook.Worksheets.RemoveAt(workbook.Worksheets.Count - 1); // удаляем последний пустой лист
                } finally {
                    workbook.EndUpdate();
                }
                workbook.Calculate();
                workbook.SaveDocument(path, DocumentFormat.OpenXml);
            }
        }

        /// <summary>
        /// Проверяет документы на конфликты имен и возвращает коллекцию документов из заданной коллекции,
        /// которые НЕ образуют конфликты.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="errorMessage">Сообщение об ошибке, или пустая строка, если ошибок нет</param>
        /// <returns></returns>
        private IList<IDocumentData> GetNotConflictedDocuments(
            ICollection<IDocumentData> data,
            out string errorMessage) {

            var docsWithNameConflicts = data
                .GroupBy(doc => CleanSheetName(doc.Title))
                .Where(group => group.Count() > 1)
                .SelectMany(group => group.ToList())
                .ToArray();
            if(docsWithNameConflicts.Length > 0) {
                errorMessage = $"{string.Join(Environment.NewLine, docsWithNameConflicts.Select(doc => doc.Title))}" +
                    $"\nЭти документы нельзя выгрузить за один раз, т.к. они образуют конфликт имен в листах Excel." +
                    "\nИмя листа Excel должно быть не более 31 символа" +
                    "\nне должно начинаться или заканчиваться с (')" +
                    "\nне должно содержать \\, /, ?, :, *, [, ] ";
            } else {
                errorMessage = string.Empty;
            }
            return data.Except(docsWithNameConflicts).ToList();
        }

        private IList<IDuctData> GetOrderedDuctData(IDocumentData documentData) {
            return documentData.Ducts
                .OrderBy(d => d.TypeName)
                .ThenBy(d => d.Name)
                .ThenBy(d => d.Size)
                .ToList();
        }

        private IList<IPipeData> GetOrderedPipeData(IDocumentData documentData) {
            return documentData.Pipes
                .OrderBy(d => d.TypeName)
                .ThenBy(d => d.Name)
                .ThenBy(d => d.Size)
                .ToList();
        }

        private IList<IPipeInsulationData> GetOrderedPipeInsulationData(IDocumentData documentData) {
            return documentData.PipeInsulations
                .OrderBy(d => d.TypeName)
                .ThenBy(d => d.Name)
                .ThenBy(d => d.PipeSize)
                .ThenBy(d => d.Thickness)
                .ToList();
        }

        /// <summary>
        /// Записывает данные по воздуховодам на заданный лист Excel, начиная с заданной строчки
        /// </summary>
        /// <param name="worksheet">Лист Excel для записи</param>
        /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
        /// <param name="ductData">Данные по воздуховодам</param>
        /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
        private int WriteDuctData(Worksheet worksheet, int startRow, IList<IDuctData> ductData) {
            worksheet.Rows[startRow][0].Value = "Воздуховоды";
            startRow++;
            worksheet.Rows[startRow][0].Value = "Тип";
            worksheet.Rows[startRow][1].Value = "ФОП_ВИС_Наименование комбинированное";
            worksheet.Rows[startRow][2].Value = "Размер";
            worksheet.Rows[startRow][3].Value = "Длина, м";
            startRow++;
            int ductsCount = ductData.Count;
            int lastRow = startRow + ductsCount - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = ductData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = ductData[row - startRow].Name;
                worksheet.Rows[row][2].Value = ductData[row - startRow].Size;
                worksheet.Rows[row][3].Value = ductData[row - startRow].Length / 1000;
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
            worksheet.Rows[startRow][0].Value = "Трубы";
            startRow++;
            worksheet.Rows[startRow][0].Value = "Тип";
            worksheet.Rows[startRow][1].Value = "ФОП_ВИС_Наименование комбинированное";
            worksheet.Rows[startRow][2].Value = "Размер";
            worksheet.Rows[startRow][3].Value = "Длина, м";
            startRow++;
            int pipesCount = pipeData.Count;
            int lastRow = startRow + pipesCount - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = pipeData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = pipeData[row - startRow].Name;
                worksheet.Rows[row][2].Value = pipeData[row - startRow].Size;
                worksheet.Rows[row][3].Value = pipeData[row - startRow].Length / 1000;
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
        private int WritePipeInsulationData(
            Worksheet worksheet,
            int startRow,
            IList<IPipeInsulationData> pipeInsulationData) {

            worksheet.Rows[startRow][0].Value = "Изоляция трубопроводов";
            startRow++;
            worksheet.Rows[startRow][0].Value = "Тип";
            worksheet.Rows[startRow][1].Value = "ФОП_ВИС_Наименование комбинированное";
            worksheet.Rows[startRow][2].Value = "Размер трубы";
            worksheet.Rows[startRow][3].Value = "Толщина, мм";
            worksheet.Rows[startRow][4].Value = "Длина, м";
            startRow++;
            int count = pipeInsulationData.Count;
            int lastRow = startRow + count - 1;
            for(int row = startRow; row < lastRow + 1; row++) {
                worksheet.Rows[row][0].Value = pipeInsulationData[row - startRow].TypeName;
                worksheet.Rows[row][1].Value = pipeInsulationData[row - startRow].Name;
                worksheet.Rows[row][2].Value = pipeInsulationData[row - startRow].PipeSize;
                worksheet.Rows[row][3].Value = pipeInsulationData[row - startRow].Thickness;
                worksheet.Rows[row][4].Value = pipeInsulationData[row - startRow].Length / 1000;
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
            var charsToRemove = new char[] { '\\', '/', '?', ':', '*', '[', ']', '\'' };
            string trimName = string.Concat(name.Trim().Take(31)).Trim();
            foreach(char charToRemove in charsToRemove) {
                trimName = trimName.Replace(charToRemove, '_');
            }
            return trimName;
        }
    }
}
