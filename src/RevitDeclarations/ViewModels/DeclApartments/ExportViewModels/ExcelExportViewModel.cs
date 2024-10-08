using System;
using System.Collections.Generic;
using System.Linq;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class ExcelExportViewModel : ExportViewModel {
        public ExcelExportViewModel(string name, Guid id, DeclarationSettings settings) 
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> apartments) {
            ApartDeclTableInfo tableData = new ApartDeclTableInfo(apartments.Cast<Apartment>().ToList(), _settings);
            ApartDeclDataTable table = new ApartDeclDataTable(tableData, _settings);

            ExportTable<ExcelExporter>(path, table);
        }
    }
}
