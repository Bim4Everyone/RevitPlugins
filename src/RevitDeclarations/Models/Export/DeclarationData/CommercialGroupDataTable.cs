using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialGroupDataTable : IDeclarationDataTable {
        private readonly CommercialGroupTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;
        private readonly List<IDeclarationDataTable> _subTables = new List<IDeclarationDataTable>();

        public CommercialGroupDataTable(CommercialGroupTableInfo tableInfo, DeclarationSettings settings) {
            _tableInfo = tableInfo;
            _settings = settings;

            _mainTable = new DataTable();
            _headerTable = new DataTable();

            CreateColumns();
            SetDataTypesForColumns();
            CreateRows();

            FillTableRoomsHeader();
            FillTableRoomsInfo();
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
            int roomsNumber = _tableInfo.RoomGroups.First().Rooms.Count();
            for(int i = 0; i < roomsNumber; i++) {
                _mainTable.Rows.Add();
            }

            _headerTable.Rows.Add();
        }

        private void SetDataTypesForColumns() {
            _mainTable.Columns[1].DataType = typeof(double);
        }

        private void FillTableRoomsHeader() {
            _headerTable.Rows[0][0] = "Наименование помещения";
            _headerTable.Rows[0][1] = "Площадь, м²";
            _headerTable.Rows[0][2] = "Условный номер";
        }

        private void FillTableRoomsInfo() {
            int rowNumber = 0;

            CommercialRooms commercialRooms = _tableInfo.RoomGroups
                .Cast<CommercialRooms>()
                .First();

            foreach(RoomElement room in commercialRooms.Rooms) {
                _mainTable.Rows[rowNumber][0] = room.Name;
                _mainTable.Rows[rowNumber][1] = room.Area;
                _mainTable.Rows[rowNumber][2] = commercialRooms.DeclarationNumber;

                rowNumber++;
            }
        }
    }
}
