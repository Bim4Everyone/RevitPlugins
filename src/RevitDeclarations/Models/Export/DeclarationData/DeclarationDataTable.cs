using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal abstract class DeclarationDataTable : IDeclarationDataTable {
        private protected readonly ITableInfo _tableInfo;
        private protected readonly DeclarationSettings _settings;
        private protected readonly DataTable _mainTable;
        private protected readonly DataTable _headerTable;
        private protected readonly List<IDeclarationDataTable> _subTables = new List<IDeclarationDataTable>();

        protected DeclarationDataTable(ITableInfo tableInfo) {
            _tableInfo = tableInfo;
            _settings = tableInfo.Settings;

            _mainTable = new DataTable();
            _headerTable = new DataTable();
            CreateColumns();
        }

        public DataTable MainDataTable => _mainTable;
        public DataTable HeaderDataTable => _headerTable;
        public ITableInfo TableInfo => _tableInfo;

        public List<IDeclarationDataTable> SubTables => _subTables;

        private protected void CreateColumns() {
            for(int i = 0; i <= _tableInfo.FullTableWidth; i++) {
                _mainTable.Columns.Add();
                _headerTable.Columns.Add();
            }
        }

        private protected void CreateRows() {
            for(int i = 0; i < _tableInfo.RoomGroups.Count; i++) {
                _mainTable.Rows.Add();
            }

            _headerTable.Rows.Add();
        }

        private protected void SetTypeForColumns(int[] columnIndexes) {
            foreach(int i in columnIndexes) {
                _mainTable.Columns[i].DataType = typeof(double);
            }
        }
    }

}
