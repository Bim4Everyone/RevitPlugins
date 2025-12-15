using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;

internal class PublicAreasCsvExportVM : ExportViewModel {
    public PublicAreasCsvExportVM(string name, Guid id, DeclarationSettings settings)
            : base(name, id, settings) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var publicAreas = roomGroups.Cast<PublicArea>().ToList();
        var tableData = new PublicAreasTableInfo(publicAreas, _settings);
        var table = new PublicAreasDataTable("16.1 МОП", tableData);
        table.GenerateTable();

        ExportTable<ExcelExporter>(path, table);
    }
}
