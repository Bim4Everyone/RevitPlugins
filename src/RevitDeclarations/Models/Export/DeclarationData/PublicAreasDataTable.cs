using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models { 
    internal class PublicAreasDataTable : IDeclarationDataTable {
        private readonly PublicAreasTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;
        private readonly List<IDeclarationDataTable> _subTables = new List<IDeclarationDataTable>();

        public PublicAreasDataTable(PublicAreasTableInfo tableInfo) {
            _tableInfo = tableInfo;
            _settings = tableInfo.Settings;

            _mainTable = new DataTable();
            _headerTable = new DataTable();

            CreateColumns();
            SetDataTypesForColumns();
            CreateRows();

            FillTableApartmentHeader();
            FillTableApartmentsInfo();
        }

        public DataTable MainDataTable => _mainTable;
        public DataTable HeaderDataTable => _headerTable;
        public ITableInfo TableInfo => _tableInfo;
        public List<IDeclarationDataTable> SubTables => _subTables;

        private void CreateColumns() {
            for(int i = 0; i <= _tableInfo.FullTableWidth; i++) {
                _mainTable.Columns.Add();
                _headerTable.Columns.Add();
            }
        }

        private void CreateRows() {
            for(int i = 0; i < _tableInfo.RoomGroups.Count; i++) {
                _mainTable.Rows.Add();
            }

            _headerTable.Rows.Add();
        }

        private void SetDataTypesForColumns() {
            _mainTable.Columns[4].DataType = typeof(double);
        }
        private void FillTableApartmentHeader() {
            _headerTable.Rows[0][0] = "№ п/п";
            _headerTable.Rows[0][1] = "Вид помещения";
            _headerTable.Rows[0][2] = "Описание места расположения помещения";
            _headerTable.Rows[0][3] = "Назначение помещения";
            _headerTable.Rows[0][4] = "Площадь, м²";
            _headerTable.Rows[0][5] = "ИД объекта";
        }
        private void FillTableApartmentsInfo() {
            int rowNumber = 0;

            foreach(PublicArea publicArea in _tableInfo.RoomGroups.Cast<PublicArea>()) {
                _mainTable.Rows[rowNumber][0] = publicArea.DeclarationNumber;
                _mainTable.Rows[rowNumber][1] = publicArea.GroupName;
                _mainTable.Rows[rowNumber][2] = publicArea.RoomPosition;
                _mainTable.Rows[rowNumber][3] = publicArea.Department;
                _mainTable.Rows[rowNumber][4] = publicArea.AreaMain;
                _mainTable.Rows[rowNumber][5] = "";

                rowNumber++;
            }
        }
    }
}
