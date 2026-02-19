using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using ClosedXML.Excel;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitBatchSpecExport.Models;

using SaveOptions = ClosedXML.Excel.SaveOptions;

namespace RevitBatchSpecExport.Services;

internal class DataExporter : IDataExporter {
    private readonly IConstantsProvider _constantsProvider;
    private readonly ICopyNameProvider _copyNameProvider;
    private readonly IErrorMessagesProvider _errorMessagesProvider;
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _repository;

    public DataExporter(
        RevitRepository repository,
        ICopyNameProvider copyNameProvider,
        IConstantsProvider constantsProvider,
        IErrorMessagesProvider errorMessagesProvider,
        ILocalizationService localizationService) {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _copyNameProvider = copyNameProvider ?? throw new ArgumentNullException(nameof(copyNameProvider));
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
        _errorMessagesProvider = errorMessagesProvider
                                 ?? throw new ArgumentNullException(nameof(errorMessagesProvider));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public void ExportData(
        DirectoryInfo docExportDirectory,
        Document document,
        PluginConfig config) {
        if(docExportDirectory is null) {
            throw new ArgumentNullException(nameof(docExportDirectory));
        }

        if(document is null) {
            throw new ArgumentNullException(nameof(document));
        }

        var revitSheets = _repository.GetSheetModels(document);

        if(revitSheets.Count == 0) {
            return;
        }

        foreach(var revitSheet in revitSheets) {
            string path = CreateExcelFileName(docExportDirectory, revitSheet, config);
            using var workbook = new XLWorkbook();
            for(int sheetIndex = 0; sheetIndex < revitSheet.Schedules.Count; sheetIndex++) {
                var schedule = revitSheet.Schedules[sheetIndex].Schedule;
                string name = CleanSheetName(schedule.Title);
                var worksheet = workbook.Worksheets.Add(name);

                Write(worksheet, schedule);

                worksheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(path);
        }
    }

    /// <summary>
    /// Создает абсолютный путь для нового файла выгрузки
    /// </summary>
    /// <param name="exportDirectory">Директория для выгрузки</param>
    /// <param name="sheet">Экспортируемый лист Revit</param>
    /// <param name="config">Настройки</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    private string CreateExcelFileName(DirectoryInfo exportDirectory, SheetModel sheet, PluginConfig config) {
        if(exportDirectory is null) {
            throw new ArgumentNullException(nameof(exportDirectory));
        }

        const string fileExtension = ".xlsx";
        string paramValue =
            sheet.Sheet.IsExistsParam(config.SheetParamName) && sheet.Sheet.IsExistsParamValue(config.SheetParamName)
                ? sheet.Sheet.GetParamValueOrDefault<string>(config.SheetParamName)
                : string.Empty;
        string sheetNumber = sheet.Sheet.SheetNumber;
        string sheetName = sheet.Sheet.Name;
        string docShortName = $"{paramValue}_{sheetNumber}_{sheetName}";
        string docLongName = $"{exportDirectory.FullName}\\{docShortName}{fileExtension}";

        if(File.Exists(docLongName)) {
            string[] neighboringFilesNames = exportDirectory
                                                 .GetFiles()
                                                 .Select(f => Path.GetFileNameWithoutExtension(f.FullName))
                                                 .ToArray()
                                             ?? [];
            docShortName = _copyNameProvider.CreateCopyName(docShortName, neighboringFilesNames);
        }

        return $"{exportDirectory.FullName}\\{docShortName}{fileExtension}";
    }

    /// <summary>
    /// Корректирует название листа Excel в соответствии с правилами
    /// https://support.microsoft.com/en-us/office/rename-a-worksheet-3f1f7148-ee83-404d-8ef0-9ff99fbad1f9
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string CleanSheetName(string name) {
        var charsToRemove = _constantsProvider.ProhibitedExcelChars;
        string trimName = new string(name.Trim().Take(_constantsProvider.DocNameMaxLength).ToArray()).Trim();
        foreach(char charToRemove in charsToRemove) {
            trimName = trimName.Replace(charToRemove, '_');
        }

        return trimName;
    }

    /// <summary>
    /// Записывает данные из спецификации на лист
    /// </summary>
    /// <param name="worksheet">Лист Excel</param>
    /// <param name="schedule">Спецификация Revit</param>
    private void Write(IXLWorksheet worksheet, ViewSchedule schedule) {
        var document = schedule.Document;
        var tableData = schedule.GetTableData();

        AlignCells(worksheet, tableData);

        int currentRow = 1;
        currentRow = ExportSection(worksheet, schedule, document, tableData, SectionType.Header, currentRow);
        ExportSection(worksheet, schedule, document, tableData, SectionType.Body, currentRow);
    }

    private int ExportSection(
        IXLWorksheet worksheet,
        ViewSchedule schedule,
        Document document,
        TableData tableData,
        SectionType sectionType,
        int startRow) {
        var sectionData = tableData.GetSectionData(sectionType);

        int numberOfRows = sectionData.NumberOfRows;
        int numberOfColumns = sectionData.NumberOfColumns;
        int firstRowNumber = sectionData.FirstRowNumber;
        int firstColumnNumber = sectionData.FirstColumnNumber;

        int currentRow = startRow;
        for(int row = firstRowNumber; row < firstRowNumber + numberOfRows; row++) {
            double rowHeight = sectionData.GetRowHeightInPixels(row);
            worksheet.Row(currentRow).Height = rowHeight;

            int currentColumn = 1;
            for(int column = firstColumnNumber; column < numberOfColumns + firstColumnNumber; column++) {
                var mergedCells = sectionData.GetMergedCell(row, column);

                if(mergedCells.Top == row
                   && mergedCells.Left == column) {
                    string cellValue = schedule.GetCellText(sectionType, row, column);
                    var scheduleCell = new ScheduleCell(document, sectionData, row, column, cellValue);
                    var excelCell = worksheet.Cell(currentRow, currentColumn);

                    scheduleCell.ExportCell(excelCell);
                    scheduleCell.SetCellStyle(excelCell);

                    int rowRange = mergedCells.Bottom - row;
                    int columnRange = mergedCells.Right - column;
                    if(rowRange > 0
                       || columnRange > 0) {
                        worksheet.Range(currentRow, currentColumn, currentRow + rowRange, currentColumn + columnRange)
                            .Merge();
                    }
                }

                currentColumn++;
            }

            currentRow++;
        }

        return currentRow;
    }

    private void AlignCells(IXLWorksheet worksheet, TableData tableData) {
        var tableSection = tableData.GetSectionData(SectionType.Body);
        int numberOfColumns = tableSection.NumberOfColumns;

        for(int i = 0; i < numberOfColumns; i++) {
            double width = tableSection.GetColumnWidth(i);
            width = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters);
            worksheet.Column(i + 1).Width = width;
        }
    }
}
