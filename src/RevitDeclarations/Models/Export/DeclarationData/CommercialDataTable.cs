using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialDataTable : IDeclarationDataTable {
        private readonly CommercialTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;

        public CommercialDataTable(CommercialTableInfo tableInfo, DeclarationSettings settings) {
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
        public ITableInfo TableInfo => _tableInfo;

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
            _mainTable.Columns[7].DataType = typeof(double);
            _mainTable.Columns[8].DataType = typeof(double);
        }

        private void FillTableApartmentHeader() {
            _headerTable.Rows[0][0] = "Номер помещения";
            _headerTable.Rows[0][1] = "Назначение";
            _headerTable.Rows[0][2] = "Этаж расположения";
            _headerTable.Rows[0][3] = "Номер подъезда";
            _headerTable.Rows[0][4] = "Номер корпуса";
            _headerTable.Rows[0][5] = "Номер здания";
            _headerTable.Rows[0][6] = "Номер объекта строительства";
            _headerTable.Rows[0][7] = "Общая площадь, м²";
            _headerTable.Rows[0][8] = "Высота потолков, м";
            _headerTable.Rows[0][9] = "Тип расположения";
            _headerTable.Rows[0][10] = "Класс машиноместа";
            _headerTable.Rows[0][11] = "Наименование помещения";
            _headerTable.Rows[0][12] = "ИД объекта";
        }

        private void FillTableApartmentsInfo() {
            int rowNumber = 0;

            foreach(CommercialRooms commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
                _mainTable.Rows[rowNumber][0] = commercialRooms.Number;
                _mainTable.Rows[rowNumber][1] = commercialRooms.Department;
                _mainTable.Rows[rowNumber][2] = commercialRooms.Level;
                _mainTable.Rows[rowNumber][3] = commercialRooms.Section;
                _mainTable.Rows[rowNumber][4] = commercialRooms.Building;
                _mainTable.Rows[rowNumber][5] = commercialRooms.BuildingNumber;
                _mainTable.Rows[rowNumber][6] = commercialRooms.ConstrWorksNumber;
                _mainTable.Rows[rowNumber][7] = commercialRooms.AreaMain;
                _mainTable.Rows[rowNumber][8] = commercialRooms.RoomsHeight;
                _mainTable.Rows[rowNumber][9] = "";
                _mainTable.Rows[rowNumber][10] = "";
                _mainTable.Rows[rowNumber][11] = commercialRooms.GroupName;
                _mainTable.Rows[rowNumber][12] = _settings.ProjectName;

                rowNumber++;
            }
        }
    }
}
