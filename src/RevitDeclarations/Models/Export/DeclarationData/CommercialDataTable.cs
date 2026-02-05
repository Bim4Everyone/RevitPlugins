using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RevitDeclarations.Models;
internal class CommercialDataTable : DeclarationDataTable {
    public CommercialDataTable(string name, CommercialTableInfo tableInfo) : base(name, tableInfo) {
    }

    protected override void FillTableHeader() {
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
        _headerTable.Rows[0][11] = "Применимость";
        _headerTable.Rows[0][12] = "Наименование помещения";
        _headerTable.Rows[0][13] = "ИД объекта";
    }

    protected override void FillMainTable() {
        int rowNumber = 0;

        foreach(var commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
            _mainTable.Rows[rowNumber][0] = commercialRooms.DeclarationNumber ?? "";
            _mainTable.Rows[rowNumber][1] = commercialRooms.Department ?? "";
            _mainTable.Rows[rowNumber][2] = commercialRooms.Level ?? "";
            _mainTable.Rows[rowNumber][3] = commercialRooms.Section ?? "";
            _mainTable.Rows[rowNumber][4] = commercialRooms.Building ?? "";
            _mainTable.Rows[rowNumber][5] = commercialRooms.BuildingNumber ?? "";
            _mainTable.Rows[rowNumber][6] = commercialRooms.ConstrWorksNumber ?? "";
            _mainTable.Rows[rowNumber][7] = commercialRooms.AreaMain;
            _mainTable.Rows[rowNumber][8] = commercialRooms.RoomsHeight;
            _mainTable.Rows[rowNumber][9] = "";
            _mainTable.Rows[rowNumber][10] = commercialRooms.ParkingSpaceClass ?? "";
            _mainTable.Rows[rowNumber][11] = commercialRooms.ParkingInfo ?? "";
            _mainTable.Rows[rowNumber][12] = commercialRooms.GroupName ?? "";
            _mainTable.Rows[rowNumber][13] = _settings.ProjectName ?? "";

            rowNumber++;
        }
    }

    protected override void FillAdditionalInfo() {
        GenerateSubTables();
    }

    private void GenerateSubTables() {
        List<CommercialRooms> rooms = [];
        foreach(var commercialRooms in _tableInfo.RoomGroups.Cast<CommercialRooms>()) {
            if(!commercialRooms.IsOneRoomGroup) {
                rooms.Add(commercialRooms);
            }
        }

        var tableInfo = new CommercialGroupTableInfo(rooms, _settings);
        var table =
            new CommercialGroupDataTable("15.3 Части нежилых помещения", tableInfo);
        table.GenerateTable();

        _subTables.Add(table);
    }
}
