using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RevitDeclarations.Models;
internal class ApartmentsDataTable : DeclarationDataTable {
    public ApartmentsDataTable(string name, ApartmentsTableInfo tableInfo) : base(name, tableInfo) {
    }

    protected override void FillTableHeader() {
        _headerTable.Rows[0][0] = "Сквозной номер квартиры";
        _headerTable.Rows[0][1] = "Назначение";
        _headerTable.Rows[0][2] = "Этаж расположения";
        _headerTable.Rows[0][3] = "Номер подъезда";
        _headerTable.Rows[0][4] = "Номер корпуса";
        _headerTable.Rows[0][5] = "Номер здания";
        _headerTable.Rows[0][6] = "Номер ОКС";
        _headerTable.Rows[0][7] = "Общая площадь с пониж. коэффициентом, м²";
        _headerTable.Rows[0][8] = "Количество комнат";
        _headerTable.Rows[0][9] = "Общая жилая площадь, м²";
        _headerTable.Rows[0][10] = "Высота потолков, м";
        _headerTable.Rows[0][11] = "ИД Объекта";
        _headerTable.Rows[0][12] = "Номер на площадке";
        _headerTable.Rows[0][13] = "Общая площадь без пониж. коэффициента, м²";
        _headerTable.Rows[0][14] = "Площадь квартиры без летних помещений, м²";

        if(_settings.LoadUtp) {
            _headerTable.Rows[0][_tableInfo.UtpStart] = "Две ванны";
            _headerTable.Rows[0][_tableInfo.UtpStart + 1] = "Хайфлет";
            _headerTable.Rows[0][_tableInfo.UtpStart + 2] = "Лоджия/ балкон";
            _headerTable.Rows[0][_tableInfo.UtpStart + 3] = "Доп. летние помещения";
            _headerTable.Rows[0][_tableInfo.UtpStart + 4] = "Терраса";
            _headerTable.Rows[0][_tableInfo.UtpStart + 5] = "Мастер-спальня";
            _headerTable.Rows[0][_tableInfo.UtpStart + 6] = "Гардеробная";
            _headerTable.Rows[0][_tableInfo.UtpStart + 7] = "Постирочная";
            _headerTable.Rows[0][_tableInfo.UtpStart + 8] = "Увеличенная площадь балкона/ лоджии";
        }
    }

    private void FillTableRoomsHeader() {
        int columnNumber = _tableInfo.GroupsInfoColumnsNumber;

        foreach(var priority in _settings.UsedPriorities) {
            if(priority.IsSummer) {
                for(int k = 0; k < priority.MaxRoomAmount; k++) {
                    int columnIndex = columnNumber + k * ApartmentsTableInfo.SummerRoomCells;
                    _headerTable.Rows[0][columnIndex] = "№ Пом.";
                    _headerTable.Rows[0][columnIndex + 1] = "Наименование на планировке";
                    _headerTable.Rows[0][columnIndex + 2] = $"{priority.Name} {k + 1}, площадь без коэф.";
                    _headerTable.Rows[0][columnIndex + 3] = $"{priority.Name} {k + 1}, площадь с коэф.";
                }
                columnNumber += priority.MaxRoomAmount * ApartmentsTableInfo.SummerRoomCells;
            } else {
                for(int k = 0; k < priority.MaxRoomAmount; k++) {
                    int columnIndex = columnNumber + k * ApartmentsTableInfo.MainRoomCells;
                    _headerTable.Rows[0][columnIndex] = "№ Пом.";
                    _headerTable.Rows[0][columnIndex + 1] = "Наименование на планировке";
                    _headerTable.Rows[0][columnIndex + 2] = $"{priority.Name} {k + 1}";
                }
                columnNumber += priority.MaxRoomAmount * ApartmentsTableInfo.MainRoomCells;
            }
        }
    }

    protected override void FillMainTable() {
        int rowNumber = 0;

        foreach(Apartment apartment in _tableInfo.RoomGroups) {
            _mainTable.Rows[rowNumber][0] = apartment.FullNumber ?? "";
            _mainTable.Rows[rowNumber][1] = apartment.Department ?? "";
            _mainTable.Rows[rowNumber][2] = apartment.Level ?? "";
            _mainTable.Rows[rowNumber][3] = apartment.Section ?? "";
            _mainTable.Rows[rowNumber][4] = apartment.Building ?? "";
            _mainTable.Rows[rowNumber][5] = apartment.BuildingNumber ?? "";
            _mainTable.Rows[rowNumber][6] = apartment.ConstrWorksNumber ?? "";
            _mainTable.Rows[rowNumber][7] = apartment.AreaCoef;
            _mainTable.Rows[rowNumber][8] = apartment.RoomsAmount;
            _mainTable.Rows[rowNumber][9] = apartment.AreaLiving;
            _mainTable.Rows[rowNumber][10] = apartment.RoomsHeight;
            _mainTable.Rows[rowNumber][11] = _settings.ProjectName ?? "";
            _mainTable.Rows[rowNumber][12] = apartment.Number ?? "";
            _mainTable.Rows[rowNumber][13] = apartment.AreaMain;
            _mainTable.Rows[rowNumber][14] = apartment.AreaNonSummer;

            int columnNumber = _tableInfo.GroupsInfoColumnsNumber;

            foreach(var priority in _settings.UsedPriorities) {
                var rooms = apartment.GetRoomsByPrior(priority);

                columnNumber = priority.IsSummer
                    ? FillSummerRoomsByPriority(priority, rooms, rowNumber, columnNumber)
                    : FillMainRoomsByPriority(priority, rooms, rowNumber, columnNumber);
            }

            rowNumber++;
        }
    }

    protected override void FillAdditionalInfo() {
        FillTableRoomsHeader();
        if(_settings.LoadUtp) {
            FillTableUtpInfo();
        }
    }

    private void FillTableUtpInfo() {
        int rowNumber = 0;
        int columnNumber = _tableInfo.UtpStart;

        foreach(Apartment apartment in _tableInfo.RoomGroups) {
            _mainTable.Rows[rowNumber][columnNumber] = apartment.UtpTwoBaths;
            _mainTable.Rows[rowNumber][columnNumber + 1] = apartment.UtpHighflat;
            _mainTable.Rows[rowNumber][columnNumber + 2] = apartment.UtpBalcony;
            _mainTable.Rows[rowNumber][columnNumber + 3] = apartment.UtpExtraSummerRooms;
            _mainTable.Rows[rowNumber][columnNumber + 4] = apartment.UtpTerrace;
            _mainTable.Rows[rowNumber][columnNumber + 5] = apartment.UtpMasterBedroom;
            _mainTable.Rows[rowNumber][columnNumber + 6] = apartment.UtpPantry;
            _mainTable.Rows[rowNumber][columnNumber + 7] = apartment.UtpLaundry;
            _mainTable.Rows[rowNumber][columnNumber + 8] = apartment.UtpExtraBalconyArea;

            rowNumber++;
        }
    }

    private int FillSummerRoomsByPriority(RoomPriority priority,
                                          IReadOnlyList<RoomElement> rooms,
                                          int rowNumber,
                                          int startColumn) {
        for(int k = 0; k < priority.MaxRoomAmount; k++) {
            if(rooms.ElementAtOrDefault(k) != null) {
                int columnIndex = startColumn + k * ApartmentsTableInfo.SummerRoomCells;
                _mainTable.Rows[rowNumber][columnIndex] = rooms[k].Number ?? "";
                _mainTable.Rows[rowNumber][columnIndex + 1] = rooms[k].DeclarationName ?? "";
                _mainTable.Rows[rowNumber][columnIndex + 2] = rooms[k].Area;
                _mainTable.Rows[rowNumber][columnIndex + 3] = rooms[k].AreaCoef;
            }
        }

        startColumn += priority.MaxRoomAmount * ApartmentsTableInfo.SummerRoomCells;
        return startColumn;
    }

    private int FillMainRoomsByPriority(RoomPriority priority,
                                        IReadOnlyList<RoomElement> rooms,
                                        int rowNumber,
                                        int startColumn) {
        for(int k = 0; k < priority.MaxRoomAmount; k++) {
            if(rooms.ElementAtOrDefault(k) != null) {
                int columnIndex = startColumn + k * ApartmentsTableInfo.MainRoomCells;
                _mainTable.Rows[rowNumber][columnIndex] = rooms[k].Number ?? "";
                _mainTable.Rows[rowNumber][columnIndex + 1] = rooms[k].DeclarationName ?? "";
                _mainTable.Rows[rowNumber][columnIndex + 2] = rooms[k].Area;
            }
        }

        startColumn += priority.MaxRoomAmount * ApartmentsTableInfo.MainRoomCells;
        return startColumn;
    }

}
