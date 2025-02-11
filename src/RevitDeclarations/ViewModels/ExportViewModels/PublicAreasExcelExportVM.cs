using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels
{
    internal class PublicAreasExcelExportVM : ExportViewModel {
        public PublicAreasExcelExportVM(string name, Guid id, DeclarationSettings settings)
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
            List<PublicArea> publicAreas = roomGroups.Cast<PublicArea>().ToList();
            PublicAreasTableInfo tableData = new PublicAreasTableInfo(publicAreas, _settings);
            PublicAreasDataTable table = new PublicAreasDataTable("16.1 МОП", tableData);
            table.GenerateTable();

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
