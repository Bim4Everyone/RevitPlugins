using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class ApartmentsExcelExportVM : ExportViewModel {
    public ApartmentsExcelExportVM(string name, Guid id, DeclarationSettings settings)
        : base(name, id, settings) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var apartments = roomGroups.Cast<Apartment>().ToList();
        var tableData = new ApartmentsTableInfo(apartments, _settings);
        var table = new ApartmentsDataTable("15.2 Жилые помещения", tableData);
        table.GenerateTable();

        ExportTable<ExcelExporter>(path, table);
    }
}
