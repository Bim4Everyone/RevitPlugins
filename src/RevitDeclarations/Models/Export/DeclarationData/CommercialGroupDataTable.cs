using System.Data;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class CommercialGroupDataTable : DeclarationDataTable {
        public CommercialGroupDataTable(CommercialGroupTableInfo tableInfo) : base(tableInfo) {
            FillTableHeader();
            FillMainTable();
        }

        private void FillTableHeader() {
            _headerTable.Rows[0][0] = "Наименование частей нежилых помещений";
            _headerTable.Rows[0][1] = "Площадь частей нежилых помещений, м²";
            _headerTable.Rows[0][2] = "Условный номер";
        }

        private void FillMainTable() {
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
