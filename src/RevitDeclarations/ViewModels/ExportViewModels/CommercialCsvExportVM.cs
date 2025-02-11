using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class CommercialCsvExportVM : ExportViewModel {
        public CommercialCsvExportVM(string name, Guid id, DeclarationSettings settings)
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
            List<CommercialRooms> commercialRooms = roomGroups.Cast<CommercialRooms>().ToList();
            CommercialTableInfo tableData = new CommercialTableInfo(commercialRooms, _settings);
            CommercialDataTable table = new CommercialDataTable("15.3 Нежилые помещения", tableData);
            table.GenerateTable();

            ExportTable<CsvExporter>(path, table);
        }
    }
}
