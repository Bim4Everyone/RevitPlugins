using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class ApartmentsExcelExportVM : ExportViewModel {
        public ApartmentsExcelExportVM(string name, Guid id, DeclarationSettings settings) 
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> apartments) {
            ApartmentsTableInfo tableData = new ApartmentsTableInfo(apartments.Cast<Apartment>().ToList(), _settings);
            ApartmentsDataTable table = new ApartmentsDataTable(tableData);

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
