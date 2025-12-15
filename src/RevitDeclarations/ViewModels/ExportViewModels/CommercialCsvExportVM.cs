using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class CommercialCsvExportVM : ExportViewModel {
    public CommercialCsvExportVM(string name, Guid id, DeclarationSettings settings)
        : base(name, id, settings) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var commercialRooms = roomGroups.Cast<CommercialRooms>().ToList();
        var tableData = new CommercialTableInfo(commercialRooms, _settings);
        var table = new CommercialDataTable("15.3 Нежилые помещения", tableData);
        table.GenerateTable();

        ExportTable<CsvExporter>(path, table);
    }
}
