using System.Data;
using System.Linq;

namespace RevitDeclarations.Models;
internal class PublicAreasDataTable : DeclarationDataTable {
    public PublicAreasDataTable(string name, PublicAreasTableInfo tableInfo) : base(name, tableInfo) {
    }

    protected override void FillTableHeader() {
        _headerTable.Rows[0][0] = "№ п/п";
        _headerTable.Rows[0][1] = "Вид помещения";
        _headerTable.Rows[0][2] = "Описание места расположения помещения";
        _headerTable.Rows[0][3] = "Назначение помещения";
        _headerTable.Rows[0][4] = "Площадь, м²";
        _headerTable.Rows[0][5] = "ИД объекта";
    }
    protected override void FillMainTable() {
        int rowNumber = 0;

        foreach(var publicArea in _tableInfo.RoomGroups.Cast<PublicArea>()) {
            _mainTable.Rows[rowNumber][0] = publicArea.DeclarationNumber ?? "";
            _mainTable.Rows[rowNumber][1] = publicArea.GroupName ?? "";
            _mainTable.Rows[rowNumber][2] = publicArea.RoomPosition ?? "";
            _mainTable.Rows[rowNumber][3] = publicArea.Department ?? "";
            _mainTable.Rows[rowNumber][4] = publicArea.AreaMain;
            _mainTable.Rows[rowNumber][5] = _settings.ProjectName ?? "";

            rowNumber++;
        }
    }

    protected override void FillAdditionalInfo() {
    }
}
