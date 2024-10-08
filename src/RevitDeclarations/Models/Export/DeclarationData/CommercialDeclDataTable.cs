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
            CreateColumns();
            SetDataTypesForColumns();
            CreateRows();

            FillTableApartmentHeader();
            FillTableApartmentsInfo();
        }

        public DataTable MainDataTable => _mainTable;
        public DataTable HeaderDataTable => _headerTable;

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
            _mainTable.Columns[5].DataType = typeof(double);
            _mainTable.Columns[6].DataType = typeof(double);
        }

        private void FillTableApartmentHeader() {
            _headerTable.Rows[0][0] = "Номер помещения";
            _headerTable.Rows[0][1] = "Назначение помещения";
            _headerTable.Rows[0][2] = "Этаж расположения";
            _headerTable.Rows[0][3] = "Номер подъезда";
            _headerTable.Rows[0][4] = "Номер корпуса";
            _headerTable.Rows[0][5] = "Общая площадь, м²";
            _headerTable.Rows[0][6] = "Высота потолков, м";
            _headerTable.Rows[0][7] = "Тип расположения";
            _headerTable.Rows[0][8] = "Класс машиноместа";
            _headerTable.Rows[0][9] = "Наименование помещения";
            _headerTable.Rows[0][10] = "ID объекта";
        }

        private void FillTableApartmentsInfo() {
            int rowNumber = 0;

            foreach(CommercialRooms commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
                // ИСПРАВИТЬ!!!
                RoomElement roomElement = commercialRooms.Rooms.First();

                _mainTable.Rows[rowNumber][0] = $"{commercialRooms.Number}_{roomElement.Number}";
                _mainTable.Rows[rowNumber][1] = commercialRooms.Department;
                _mainTable.Rows[rowNumber][2] = commercialRooms.Level;
                _mainTable.Rows[rowNumber][3] = commercialRooms.Section;
                _mainTable.Rows[rowNumber][4] = commercialRooms.Building;
                _mainTable.Rows[rowNumber][5] = commercialRooms.AreaMain;
                _mainTable.Rows[rowNumber][6] = commercialRooms.RoomsHeight;
                _mainTable.Rows[rowNumber][7] = "";
                _mainTable.Rows[rowNumber][8] = "";
                _mainTable.Rows[rowNumber][9] = roomElement.Name;
                _mainTable.Rows[rowNumber][10] = "";

                rowNumber++;
            }
        }
    }
}
