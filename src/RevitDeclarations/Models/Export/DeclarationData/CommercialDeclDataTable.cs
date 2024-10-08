using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models.Export.DeclarationData {
    internal class CommercialDeclDataTable : IDeclarationDataTable {
        private readonly CommercialDeclTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;

        public CommercialDeclDataTable(CommercialDeclTableInfo tableInfo, DeclarationSettings settings) {
            _tableInfo = tableInfo;
            _settings = settings;

            _mainTable = new DataTable();
            _headerTable = new DataTable();
        }

        public DataTable MainDataTable => _mainTable;
        public DataTable HeaderDataTable => _headerTable;
    }
}
