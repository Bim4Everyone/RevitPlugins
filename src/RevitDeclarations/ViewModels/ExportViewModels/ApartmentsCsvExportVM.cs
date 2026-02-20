using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class ApartmentsCsvExportVM : ExportViewModel {
    public ApartmentsCsvExportVM(string name, 
                                 Guid id, 
                                 DeclarationSettings settings, 
                                 IMessageBoxService messageBoxService)
        : base(name, id, settings, messageBoxService) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var apartments = roomGroups.Cast<Apartment>().ToList();
        var tableInfo = new ApartmentsTableInfo(apartments, _settings);
        var table = new ApartmentsDataTable("15.2 Жилые помещения", tableInfo);
        table.GenerateTable();

        ExportTable<CsvExporter>(path, table);
    }
}
