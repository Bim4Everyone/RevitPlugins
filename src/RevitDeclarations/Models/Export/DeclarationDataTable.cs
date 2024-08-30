using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class DeclarationDataTable {
        private readonly ExcelTableData _excelTableData;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _table;

        public DeclarationDataTable(ExcelTableData tableData, DeclarationSettings settings) {
            _excelTableData = tableData;
            _settings = settings;

            _table = new DataTable();
            CreateColumnsAndRows();

            FillTableApartmentHeader();
            FillTableRoomsHeader();
            FillTableApartmentsInfo();
            FillTableUtpInfo();
        }

        public DataTable DataTable => _table;


        private void CreateColumnsAndRows() {
            for(int i = 0; i <= _excelTableData.FullTableWidth; i++) {
                _table.Columns.Add();
            }

            for(int i = 0; i <= _excelTableData.Apartments.Count; i++) {
                _table.Rows.Add();
            }
        }

        private void FillTableApartmentHeader() {
            _table.Rows[0][0] = "Сквозной номер квартиры";
            _table.Rows[0][1] = "Назначение";
            _table.Rows[0][2] = "Этаж расположения";
            _table.Rows[0][3] = "Номер подъезда";
            _table.Rows[0][4] = "Номер корпуса";
            _table.Rows[0][5] = "Номер на площадке";
            _table.Rows[0][6] = "Общая площадь без пониж. коэффициента, м2";
            _table.Rows[0][7] = "Общая площадь с пониж. коэффициентом, м2";
            _table.Rows[0][8] = "Общая жилая площадь, м2";
            _table.Rows[0][9] = "Количество комнат";
            _table.Rows[0][10] = "ИД Объекта";
            _table.Rows[0][11] = "Площадь квартиры без летних помещений, м2";
            _table.Rows[0][12] = "Высота потолка, м";

            if(_settings.LoadUtp) {
                _table.Rows[0][_excelTableData.UtpStart] = "Две ванны";
                _table.Rows[0][_excelTableData.UtpStart + 1] = "Хайфлет";
                _table.Rows[0][_excelTableData.UtpStart + 2] = "Лоджия/ балкон";
                _table.Rows[0][_excelTableData.UtpStart + 3] = "Доп. летние помещения";
                _table.Rows[0][_excelTableData.UtpStart + 4] = "Терраса";
                _table.Rows[0][_excelTableData.UtpStart + 5] = "Мастер-спальня";
                _table.Rows[0][_excelTableData.UtpStart + 6] = "Гардеробная";
                _table.Rows[0][_excelTableData.UtpStart + 7] = "Постирочная";
                _table.Rows[0][_excelTableData.UtpStart + 8] = "Увеличенная площадь балкона/ лоджии";
            }
        }

        private void FillTableRoomsHeader() {
            int columnNumber = ExcelTableData.InfoWidth;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _table.Rows[0][columnNumber + k * ExcelTableData.SummerRoomCells] = "№ Пом.";
                        _table.Rows[0][columnNumber + k * ExcelTableData.SummerRoomCells + 1] = "Наименование на планировке";
                        _table.Rows[0][columnNumber + k * ExcelTableData.SummerRoomCells + 2] = $"{priority.Name}_{k + 1}, площадь без коэф.";
                        _table.Rows[0][columnNumber + k * ExcelTableData.SummerRoomCells + 3] = $"{priority.Name}_{k + 1}, площадь с коэф.";
                    }
                    columnNumber += priority.MaxRoomAmount * ExcelTableData.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _table.Rows[0][columnNumber + k * ExcelTableData.MainRoomCells] = "№ Пом.";
                        _table.Rows[0][columnNumber + k * ExcelTableData.MainRoomCells + 1] = "Наименование на планировке";
                        _table.Rows[0][columnNumber + k * ExcelTableData.MainRoomCells + 2] = $"{priority.Name}_{k + 1}";
                    }
                    columnNumber += priority.MaxRoomAmount * ExcelTableData.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentsInfo() {
            int rowNumber = 1;

            foreach(Apartment apartment in _excelTableData.Apartments) {
                _table.Rows[rowNumber][0] = apartment.FullNumber;
                _table.Rows[rowNumber][1] = apartment.Department;
                _table.Rows[rowNumber][2] = apartment.Level;
                _table.Rows[rowNumber][3] = apartment.Section;
                _table.Rows[rowNumber][4] = apartment.Building;
                _table.Rows[rowNumber][5] = apartment.Number;
                _table.Rows[rowNumber][6] = apartment.AreaMain;
                _table.Rows[rowNumber][7] = apartment.AreaCoef;
                _table.Rows[rowNumber][8] = apartment.AreaLiving;
                _table.Rows[rowNumber][9] = apartment.RoomsAmount;
                _table.Rows[rowNumber][10] = _settings.ProjectName;
                _table.Rows[rowNumber][11] = apartment.AreaNonSummer;
                _table.Rows[rowNumber][12] = apartment.RoomsHeight;

                int columnNumber = ExcelTableData.InfoWidth;

                foreach(RoomPriority priority in _settings.UsedPriorities) {
                    IReadOnlyList<RoomElement> rooms = apartment.GetRoomsByPrior(priority);

                    if(priority.IsSummer) {
                        columnNumber = FillSummerRoomsByPriority(priority, rooms, rowNumber, columnNumber);
                    } else {
                        columnNumber = FillMainRoomsByPriority(priority, rooms, rowNumber, columnNumber);
                    }
                }

                rowNumber++;
            }
        }

        private void FillTableUtpInfo() {
            int rowNumber = 1;
            int columnNumber = _excelTableData.UtpStart;

            foreach(Apartment apartment in _excelTableData.Apartments) {
                _table.Rows[rowNumber][columnNumber] = apartment.UtpTwoBaths;
                _table.Rows[rowNumber][columnNumber + 1] = apartment.UtpHighflat;
                _table.Rows[rowNumber][columnNumber + 2] = apartment.UtpBalcony;
                _table.Rows[rowNumber][columnNumber + 3] = apartment.UtpExtraSummerRooms;
                _table.Rows[rowNumber][columnNumber + 4] = apartment.UtpTerrace;
                _table.Rows[rowNumber][columnNumber + 5] = apartment.UtpMasterBedroom;
                _table.Rows[rowNumber][columnNumber + 6] = apartment.UtpPantry;
                _table.Rows[rowNumber][columnNumber + 7] = apartment.UtpLaundry;
                _table.Rows[rowNumber][columnNumber + 8] = apartment.UtpExtraBalconyArea;

                rowNumber++;
            }
        }

        private int FillSummerRoomsByPriority(RoomPriority priority,
                                              IReadOnlyList<RoomElement> rooms,
                                              int rowNumber,
                                              int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.SummerRoomCells]
                        = rooms[k].Number;
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.SummerRoomCells + 1]
                        = rooms[k].DeclarationName;
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.SummerRoomCells + 2]
                        = rooms[k].Area;
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.SummerRoomCells + 3]
                        = rooms[k].AreaCoef;
                }
            }

            startColumn += priority.MaxRoomAmount * ExcelTableData.SummerRoomCells;
            return startColumn;
        }

        private int FillMainRoomsByPriority(RoomPriority priority,
                                            IReadOnlyList<RoomElement> rooms,
                                            int rowNumber,
                                            int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.MainRoomCells]
                        = rooms[k].Number;
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.MainRoomCells + 1]
                        = rooms[k].DeclarationName;
                    _table.Rows[rowNumber][startColumn + k * ExcelTableData.MainRoomCells + 2]
                        = rooms[k].Area;
                }
            }

            startColumn += priority.MaxRoomAmount * ExcelTableData.MainRoomCells;
            return startColumn;
        }

    }
}
