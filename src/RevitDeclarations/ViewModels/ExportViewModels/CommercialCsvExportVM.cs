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

        public override void Export(string path, IEnumerable<RoomGroup> commercialRooms) {
            CommercialTableInfo tableData = new CommercialTableInfo(commercialRooms.Cast<CommercialRooms>().ToList(), _settings);
            CommercialDataTable table = new CommercialDataTable(tableData);

            ExportTable<CsvExporter>(path, table);
        }
    }
}
