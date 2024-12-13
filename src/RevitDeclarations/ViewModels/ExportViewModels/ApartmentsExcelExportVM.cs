using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class ApartmentsExcelExportVM : ExportViewModel {
        public ApartmentsExcelExportVM(string name, Guid id, DeclarationSettings settings) 
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
            List<Apartment> apartments = roomGroups.Cast<Apartment>().ToList();
            ApartmentsTableInfo tableData = new ApartmentsTableInfo(apartments, _settings);
            ApartmentsDataTable table = new ApartmentsDataTable("15.2 Жилые помещения", tableData);
            table.GenerateTable();

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
