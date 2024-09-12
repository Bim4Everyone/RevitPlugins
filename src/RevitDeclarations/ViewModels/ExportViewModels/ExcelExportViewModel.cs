using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class ExcelExportViewModel : ExportViewModel {
        public ExcelExportViewModel(string name, DeclarationSettings settings) 
            : base(name, settings) {
        }

        public override void Export(string path, IEnumerable<Apartment> apartments) {
            DeclarationTableInfo tableData = new DeclarationTableInfo(apartments.ToList(), _settings);
            DeclarationDataTable table = new DeclarationDataTable(tableData, _settings);

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
