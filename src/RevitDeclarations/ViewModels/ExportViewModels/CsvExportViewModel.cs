using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class CsvExportViewModel : ExportViewModel {
        public CsvExportViewModel(string name, Guid id, DeclarationSettings settings) 
            : base(name, id, settings) { 
        }

        public override void Export(string path, IEnumerable<Apartment> apartments) {
            DeclarationTableInfo tableData = new DeclarationTableInfo(apartments.ToList(), _settings);
            DeclarationDataTable table = new DeclarationDataTable(tableData, _settings);

            ExportTable<CsvExporter>(path, table);
        }
    }
}
