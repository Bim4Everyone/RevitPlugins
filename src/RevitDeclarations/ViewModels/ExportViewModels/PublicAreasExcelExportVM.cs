using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class PublicAreasExcelExportVM : ExportViewModel {
    public PublicAreasExcelExportVM(string name, 
                                    Guid id, 
                                    DeclarationSettings settings,
                                    ILocalizationService localizationService,
                                    IMessageBoxService messageBoxService)
        : base(name, id, settings, localizationService, messageBoxService) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var publicAreas = roomGroups.Cast<PublicArea>().ToList();
        var tableData = new PublicAreasTableInfo(publicAreas, _settings);
        var table = new PublicAreasDataTable(_declarationPublicAreasName, tableData);
        table.GenerateTable();

        ExportTable<ExcelExporter>(path, table);
    }
}
