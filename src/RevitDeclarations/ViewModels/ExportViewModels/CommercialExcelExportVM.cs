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

        public override void Export(string path, IEnumerable<RoomGroup> commercialRooms) {
            CommercialTableInfo tableInfo = 
                new CommercialTableInfo(commercialRooms.Cast<CommercialRooms>().ToList(), _settings);
            CommercialDataTable table = new CommercialDataTable(tableInfo);

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
