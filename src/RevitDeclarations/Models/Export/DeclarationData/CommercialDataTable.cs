using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialDataTable : DeclarationDataTable {
        public CommercialDataTable(CommercialTableInfo tableInfo) : base(tableInfo) {
            SetTypeForColumns(new int[]{ 7, 8 });
            CreateRows();

            FillTableRoomsHeader();
            FillTableRoomsInfo();
            GenerateSubTables();
        }

        private void FillTableRoomsHeader() {
            _headerTable.Rows[0][0] = "Условный номер";
            _headerTable.Rows[0][1] = "Назначение";
            _headerTable.Rows[0][2] = "Этаж расположения";
            _headerTable.Rows[0][3] = "Номер подъезда";
            _headerTable.Rows[0][4] = "Номер корпуса";
            _headerTable.Rows[0][5] = "Номер здания";
            _headerTable.Rows[0][6] = "Номер ОКС";
            _headerTable.Rows[0][7] = "Общая площадь, м²";
            _headerTable.Rows[0][8] = "Высота потолков, м";
            _headerTable.Rows[0][9] = "Тип расположения";
            _headerTable.Rows[0][10] = "Класс машино-места";
            _headerTable.Rows[0][11] = "Наименование помещения";
            _headerTable.Rows[0][12] = "ИД объекта";
        }

        private void FillTableRoomsInfo() {
            int rowNumber = 0;

            foreach(CommercialRooms commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
                _mainTable.Rows[rowNumber][0] = commercialRooms.DeclarationNumber;
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

        private void GenerateSubTables() {
            foreach(var commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
                if(!commercialRooms.IsOneRoomGroup) {
                    List<CommercialRooms> rooms = new List<CommercialRooms> { commercialRooms };

                    CommercialGroupTableInfo tableInfo = new CommercialGroupTableInfo(rooms, _settings);
                    CommercialGroupDataTable table = new CommercialGroupDataTable(tableInfo, _settings);

                    _subTables.Add(table);
                }
            }
        }
    }
}
