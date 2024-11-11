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

        public override void Export(string path, IEnumerable<RoomGroup> publicAreas) {
            PublicAreasTableInfo tableData = new PublicAreasTableInfo(publicAreas.Cast<PublicArea>().ToList(), _settings);
            PublicAreasDataTable table = new PublicAreasDataTable(tableData);

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
