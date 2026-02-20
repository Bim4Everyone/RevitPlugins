using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class CommercialExcelExportVM : ExportViewModel {
    public CommercialExcelExportVM(string name, 
                                   Guid id, 
                                   DeclarationSettings settings,
                                   ILocalizationService localizationService,
                                   IMessageBoxService messageBoxService)
        : base(name, id, settings, localizationService, messageBoxService) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var commercialRooms = roomGroups.Cast<CommercialRooms>().ToList();
        var tableInfo = new CommercialTableInfo(commercialRooms, _settings);
        var table = new CommercialDataTable("15.3 Нежилые помещения", tableInfo);
        table.GenerateTable();

        ExportTable<ExcelExporter>(path, table);
    }
}
