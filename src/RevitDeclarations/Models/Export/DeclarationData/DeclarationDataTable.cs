using System.Collections.Generic;
using System.Data;

namespace RevitDeclarations.Models;
internal abstract class DeclarationDataTable : IDeclarationDataTable {
    protected readonly string _name;
    protected readonly ITableInfo _tableInfo;
    protected readonly DeclarationSettings _settings;
    protected readonly DataTable _mainTable;
    protected readonly DataTable _headerTable;
    protected readonly List<IDeclarationDataTable> _subTables = [];

    protected DeclarationDataTable(string name, ITableInfo tableInfo) {
        _name = name;
        _tableInfo = tableInfo;
        _settings = tableInfo.Settings;

        _mainTable = new DataTable();
        _headerTable = new DataTable();
    }

    public string Name => _name;
    public DataTable MainDataTable => _mainTable;
    public DataTable HeaderDataTable => _headerTable;
    public ITableInfo TableInfo => _tableInfo;
    public List<IDeclarationDataTable> SubTables => _subTables;

    public void GenerateTable() {
        CreateColumns();
        SetTypeForColumns(_tableInfo.NumericColumnsIndexes);
        CreateRows();
        FillTableHeader();
        FillMainTable();
        FillAdditionalInfo();
    }

    protected void CreateColumns() {
        for(int i = 0; i <= _tableInfo.ColumnsTotalNumber; i++) {
            _mainTable.Columns.Add();
            _headerTable.Columns.Add();
        }
    }

    protected void CreateRows() {
        for(int i = 0; i < _tableInfo.RowsTotalNumber; i++) {
            _mainTable.Rows.Add();
        }

        _headerTable.Rows.Add();
    }

    protected void SetTypeForColumns(int[] columnIndexes) {
        foreach(int i in columnIndexes) {
            _mainTable.Columns[i].DataType = typeof(double);
        }
    }

    protected abstract void FillTableHeader();
    protected abstract void FillMainTable();
    protected abstract void FillAdditionalInfo();
}
