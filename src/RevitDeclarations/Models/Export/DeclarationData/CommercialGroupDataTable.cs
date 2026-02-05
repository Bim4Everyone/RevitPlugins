namespace RevitDeclarations.Models;
internal class CommercialGroupDataTable : DeclarationDataTable {
    public CommercialGroupDataTable(string name, CommercialGroupTableInfo tableInfo) : base(name, tableInfo) {
    }

    protected override void FillTableHeader() {
        _headerTable.Rows[0][0] = "Наименование частей нежилых помещений";
        _headerTable.Rows[0][1] = "Площадь частей нежилых помещений, м²";
        _headerTable.Rows[0][2] = "Условный номер";
    }

    protected override void FillMainTable() {
        int rowNumber = 0;

        foreach(CommercialRooms commercialRooms in _tableInfo.RoomGroups) {
            foreach(var room in commercialRooms.Rooms) {
                _mainTable.Rows[rowNumber][0] = room.Name ?? "";
                _mainTable.Rows[rowNumber][1] = room.Area;
                _mainTable.Rows[rowNumber][2] = commercialRooms.DeclarationNumber ?? "";

                rowNumber++;
            }
        }
    }

    protected override void FillAdditionalInfo() {
    }
}
