using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;
using RevitDeclarations.Models.Export.DeclarationData;

namespace RevitDeclarations.ViewModels {
    internal class CommercialCsvExportVM : ExportViewModel {
        public CommercialCsvExportVM(string name, Guid id, DeclarationSettings settings)
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> commercialRooms) {
            CommercialDeclTableInfo tableData = new CommercialDeclTableInfo(commercialRooms.Cast<CommercialRooms>().ToList(), _settings);
            CommercialDeclDataTable table = new CommercialDeclDataTable(tableData, _settings);

            ExportTable<CsvExporter>(path, table);
        }
    }
}
