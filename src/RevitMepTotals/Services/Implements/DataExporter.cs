using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ClosedXML.Excel;

using dosymep.SimpleServices;

using RevitMepTotals.Models;
using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Services.Implements;
internal class DataExporter : IDataExporter {
    private readonly RevitRepository _repository;
    private readonly ICopyNameProvider _copyNameProvider;
    private readonly IConstantsProvider _constantsProvider;
    private readonly IErrorMessagesProvider _errorMessagesProvider;
    private readonly ILocalizationService _localizationService;

    public DataExporter(
        RevitRepository repository,
        ICopyNameProvider copyNameProvider,
        IConstantsProvider constantsProvider,
        IErrorMessagesProvider errorMessagesProvider,
        ILocalizationService localizationService) {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _copyNameProvider = copyNameProvider
            ?? throw new ArgumentNullException(nameof(copyNameProvider));
        _constantsProvider = constantsProvider
            ?? throw new ArgumentNullException(nameof(constantsProvider));
        _errorMessagesProvider = errorMessagesProvider
            ?? throw new ArgumentNullException(nameof(errorMessagesProvider));
        _localizationService = localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public void ExportData(
        DirectoryInfo exportDirectory,
        IList<IDocumentData> dataForExport,
        out string exportError) {

        if(exportDirectory is null) { throw new ArgumentNullException(nameof(exportDirectory)); }
        if(dataForExport is null) { throw new ArgumentNullException(nameof(dataForExport)); }
        var data = GetNotConflictedDocuments(dataForExport, out string documentErrors);
        exportError = documentErrors;

        if(data.Count == 0) {
            return;
        }
        string path = CreateFileName(exportDirectory);
        // https://docs.devexpress.com/OfficeFileAPI/15072/spreadsheet-document-api/getting-started?v=21.2
        using var workbook = new XLWorkbook();
        for(int sheetIndex = 0; sheetIndex < data.Count; sheetIndex++) {
            string name = CleanSheetName(data[sheetIndex].Title);
            var worksheet = workbook.Worksheets.Add(name);

            int rowToWrite = 1;
            rowToWrite = WriteDuctData(worksheet, rowToWrite, GetOrderedDuctData(data[sheetIndex]));

            rowToWrite++;
            rowToWrite = WritePipeData(worksheet, rowToWrite, GetOrderedPipeData(data[sheetIndex]));

            rowToWrite++;
            WritePipeInsulationData(worksheet, rowToWrite, GetOrderedPipeInsulationData(data[sheetIndex]));

            worksheet.Columns().AdjustToContents();
        }
        workbook.SaveAs(path);
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
            .SelectMany(group => group)
            .ToArray();
        errorMessage = docsWithNameConflicts.Length > 0
            ? _errorMessagesProvider
                .GetFileNamesConflictMessage(docsWithNameConflicts.Select(doc => doc.Title).ToArray())
            : string.Empty;
        return [.. data.Except(docsWithNameConflicts)];
    }

    private IList<IDuctData> GetOrderedDuctData(IDocumentData documentData) {
        return [.. documentData.Ducts
            .OrderBy(d => d.SystemName)
            .ThenBy(d => d.TypeName)
            .ThenBy(d => d.Name)
            .ThenBy(d => d.Size)];
    }

    private IList<IPipeData> GetOrderedPipeData(IDocumentData documentData) {
        return [.. documentData.Pipes
            .OrderBy(d => d.SystemName)
            .ThenBy(d => d.TypeName)
            .ThenBy(d => d.Name)
            .ThenBy(d => d.Size)];
    }

    private IList<IPipeInsulationData> GetOrderedPipeInsulationData(IDocumentData documentData) {
        return [.. documentData.PipeInsulations
            .OrderBy(d => d.SystemName)
            .ThenBy(d => d.TypeName)
            .ThenBy(d => d.Name)
            .ThenBy(d => d.PipeSize)
            .ThenBy(d => d.Thickness)
            .Where(d => d.Length > 0)];
    }

    /// <summary>
    /// Записывает данные по воздуховодам на заданный лист Excel, начиная с заданной строчки
    /// </summary>
    /// <param name="worksheet">Лист Excel для записи</param>
    /// <param name="startRow">Индекс первой строчки, с которой нужно начать запись данных</param>
    /// <param name="ductData">Данные по воздуховодам</param>
    /// <returns>Индекс последней строчки, на которую были записаны данные</returns>
    private int WriteDuctData(IXLWorksheet worksheet, int startRow, IList<IDuctData> ductData) {
        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.Ducts");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.SystemName");
        worksheet.Cell(startRow, 2).Value = _localizationService.GetLocalizedString("Excel.TypeName");
        worksheet.Cell(startRow, 3).Value = _repository.CombinedNameParam.Name;
        worksheet.Cell(startRow, 4).Value = _localizationService.GetLocalizedString("Excel.Size");
        worksheet.Cell(startRow, 5).Value = _localizationService.GetLocalizedString("Excel.Length");
        worksheet.Cell(startRow, 6).Value = _localizationService.GetLocalizedString("Excel.Area");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        int ductsCount = ductData.Count;
        int lastRow = startRow + ductsCount - 1;
        for(int row = startRow; row < lastRow + 1; row++) {
            worksheet.Cell(row, 1).Value = ductData[row - startRow].SystemName;
            worksheet.Cell(row, 2).Value = ductData[row - startRow].TypeName;
            worksheet.Cell(row, 3).Value = ductData[row - startRow].Name;
            worksheet.Cell(row, 4).Value = ductData[row - startRow].Size;
            worksheet.Cell(row, 5).Value = ductData[row - startRow].Length / 1000;
            worksheet.Cell(row, 6).Value = ductData[row - startRow].Area;
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
    private int WritePipeData(IXLWorksheet worksheet, int startRow, IList<IPipeData> pipeData) {
        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.Pipes");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.SystemName");
        worksheet.Cell(startRow, 2).Value = _localizationService.GetLocalizedString("Excel.TypeName");
        worksheet.Cell(startRow, 3).Value = _repository.CombinedNameParam.Name;
        worksheet.Cell(startRow, 4).Value = _localizationService.GetLocalizedString("Excel.Size");
        worksheet.Cell(startRow, 5).Value = _localizationService.GetLocalizedString("Excel.Length");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        int pipesCount = pipeData.Count;
        int lastRow = startRow + pipesCount - 1;
        for(int row = startRow; row < lastRow + 1; row++) {
            worksheet.Cell(row, 1).Value = pipeData[row - startRow].SystemName;
            worksheet.Cell(row, 2).Value = pipeData[row - startRow].TypeName;
            worksheet.Cell(row, 3).Value = pipeData[row - startRow].Name;
            worksheet.Cell(row, 4).Value = pipeData[row - startRow].Size;
            worksheet.Cell(row, 5).Value = pipeData[row - startRow].Length / 1000;
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
        IXLWorksheet worksheet,
        int startRow,
        IList<IPipeInsulationData> pipeInsulationData) {

        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.PipeInsulation");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        worksheet.Cell(startRow, 1).Value = _localizationService.GetLocalizedString("Excel.SystemName");
        worksheet.Cell(startRow, 2).Value = _localizationService.GetLocalizedString("Excel.TypeName");
        worksheet.Cell(startRow, 3).Value = _repository.CombinedNameParam.Name;
        worksheet.Cell(startRow, 4).Value = _localizationService.GetLocalizedString("Excel.PipeSize");
        worksheet.Cell(startRow, 5).Value = _localizationService.GetLocalizedString("Excel.Thickness");
        worksheet.Cell(startRow, 6).Value = _localizationService.GetLocalizedString("Excel.Length");
        worksheet.Row(startRow).Style.Font.Bold = true;
        startRow++;
        int count = pipeInsulationData.Count;
        int lastRow = startRow + count - 1;
        for(int row = startRow; row < lastRow + 1; row++) {
            worksheet.Cell(row, 1).Value = pipeInsulationData[row - startRow].SystemName;
            worksheet.Cell(row, 2).Value = pipeInsulationData[row - startRow].TypeName;
            worksheet.Cell(row, 3).Value = pipeInsulationData[row - startRow].Name;
            worksheet.Cell(row, 4).Value = pipeInsulationData[row - startRow].PipeSize;
            worksheet.Cell(row, 5).Value = pipeInsulationData[row - startRow].Thickness;
            worksheet.Cell(row, 6).Value = pipeInsulationData[row - startRow].Length / 1000;
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

        string suffix = DateTime.Now.ToString("yyyy-MM-dd");
        const string fileExtension = ".xlsx";

        string docShortName = _localizationService.GetLocalizedString("Excel.FileNameMask", suffix);
        string docLongName = $"{exportDirectory.FullName}\\{docShortName}{fileExtension}";

        if(File.Exists(docLongName)) {
            string[] neighboringFilesNames = exportDirectory
                .GetFiles()
                .Select(f => Path.GetFileNameWithoutExtension(f.FullName))
                .ToArray() ?? [];
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
        var charsToRemove = _constantsProvider.ProhibitedExcelChars;
        string trimName = new string(name.Trim().Take(_constantsProvider.DocNameMaxLength).ToArray()).Trim();
        foreach(char charToRemove in charsToRemove) {
            trimName = trimName.Replace(charToRemove, '_');
        }
        return trimName;
    }
}
