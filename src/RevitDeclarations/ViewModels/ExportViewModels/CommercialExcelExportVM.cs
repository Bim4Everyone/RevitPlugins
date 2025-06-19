using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class CommercialExcelExportVM : ExportViewModel {
        public CommercialExcelExportVM(string name, Guid id, DeclarationSettings settings)
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
            List<CommercialRooms> commercialRooms = roomGroups.Cast<CommercialRooms>().ToList();
            CommercialTableInfo tableInfo = new CommercialTableInfo(commercialRooms, _settings);
            CommercialDataTable table = new CommercialDataTable("15.3 Нежилые помещения", tableInfo);
            table.GenerateTable();

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
