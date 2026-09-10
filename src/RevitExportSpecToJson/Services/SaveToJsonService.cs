using System.IO;

using Autodesk.Revit.DB;

using pyRevitLabs.Json.Linq;

using RevitExportSpecToJson.Models;
using RevitExportSpecToJson.Utils;

namespace RevitExportSpecToJson.Services;

internal interface ISaveToJsonService {
    void SaveToJson(
        string saveFolder,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default,
        params IEnumerable<Schedule> schedules);
}

internal sealed class SaveToJsonService : ISaveToJsonService {
    private readonly RevitRepository _revitRepository;

    public SaveToJsonService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public void SaveToJson(
        string saveFolder,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default,
        params IEnumerable<Schedule> schedules) {
        // to update schedule appearance
        using var transaction = _revitRepository.StartTransaction("SaveToJson");

        int index = 0;
        foreach(var schedule in schedules) {
            progress?.Report(++index);
            cancellationToken.ThrowIfCancellationRequested();

            var elements = GetElements(schedule.ViewSchedule);
            var jsonSchedule = new JObject {
                ["schedule_name"] = schedule.Name,
                ["schedule_type"] = schedule.TypeSchedule,
                ["elements"] = elements
            };

            string scheduleName = FileUtils.GetSafeFolderName(schedule.Name);
            File.WriteAllText(Path.Combine(saveFolder, $"{scheduleName}.json"), jsonSchedule.ToString());
        }

        // because we changed schedule appearance
        // users need old settings
        transaction.RollBack();
    }

    private static JArray GetElements(ViewSchedule viewSchedule) {
        // remove view template
        // because view template overrides style headers
        viewSchedule.ViewTemplateId = ElementId.InvalidElementId;
        
        // off tile and headers
        // to fix export column headers
        viewSchedule.Definition.ShowTitle = false;
        viewSchedule.Definition.ShowHeaders = false;
        
        var tableData = viewSchedule.GetTableData();
        var definition = viewSchedule.Definition;
        var sectionData = tableData.GetSectionData(SectionType.Body);

        string[] fields = definition.GetFieldOrder()
            .Select(scheduleFieldId => definition.GetField(scheduleFieldId))
            .Where(scheduleField => !scheduleField.IsHidden)
            .Select(scheduleField => scheduleField.GetName())
            .ToArray();

        var scheduleRows = new JArray();
        for(int rowIndex = sectionData.FirstRowNumber; rowIndex <= sectionData.LastRowNumber; rowIndex++) {
            var rowObject = new JObject();

            for(int columnIndex = sectionData.FirstColumnNumber;
                columnIndex <= sectionData.LastColumnNumber;
                columnIndex++) {
                string value = viewSchedule.GetCellText(SectionType.Body, rowIndex, columnIndex);
                if(!string.IsNullOrEmpty(value)) {
                    rowObject[fields[columnIndex]] = value;
                }
            }

            if(rowObject.Properties().Any()) {
                scheduleRows.Add(rowObject);
            }
        }

        return scheduleRows;
    }
}
