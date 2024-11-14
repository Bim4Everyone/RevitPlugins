using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class CommercialGroupDataTable : DeclarationDataTable {
        public CommercialGroupDataTable(CommercialGroupTableInfo tableInfo) : base(tableInfo) {
            FillTableRoomsHeader();
            FillTableRoomsInfo();
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
