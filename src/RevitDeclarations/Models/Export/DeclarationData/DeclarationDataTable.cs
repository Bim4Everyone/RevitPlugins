using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class DeclarationDataTable {
        private readonly DeclarationTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;

        public DeclarationDataTable(DeclarationTableInfo tableData, DeclarationSettings settings) {
            _tableInfo = tableData;
            _settings = settings;

            _mainTable = new DataTable();
            _headerTable = new DataTable();
            CreateColumns();
            SetDataTypesForColumns();
            CreateRows();

            FillTableApartmentHeader();
            FillTableRoomsHeader();
            FillTableApartmentsInfo();
            FillTableUtpInfo();
        }

        public DataTable MainDataTable => _mainTable;
        public DataTable HeaderDataTable => _headerTable;
        public DeclarationTableInfo TableInfo => _tableInfo;

        private void CreateColumns() {
            for(int i = 0; i <= _tableInfo.FullTableWidth; i++) {
                _mainTable.Columns.Add();
                _headerTable.Columns.Add();
            }
        }

        private void CreateRows() {
            for(int i = 0; i < _tableInfo.Apartments.Count; i++) {
                _mainTable.Rows.Add();
            }

            _headerTable.Rows.Add();
        }

        private void SetDataTypesForColumns() {
            _mainTable.Columns[6].DataType = typeof(double);
            _mainTable.Columns[7].DataType = typeof(double);
            _mainTable.Columns[8].DataType = typeof(double);
            _mainTable.Columns[11].DataType = typeof(double);
            _mainTable.Columns[12].DataType = typeof(double);

            int columnNumber = DeclarationTableInfo.InfoWidth;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _mainTable.Columns[columnNumber + k * DeclarationTableInfo.SummerRoomCells + 2].DataType = typeof(double);
                        _mainTable.Columns[columnNumber + k * DeclarationTableInfo.SummerRoomCells + 3].DataType = typeof(double);
                    }
                    columnNumber += priority.MaxRoomAmount * DeclarationTableInfo.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _mainTable.Columns[columnNumber + k * DeclarationTableInfo.MainRoomCells + 2].DataType = typeof(double);
                    }
                    columnNumber += priority.MaxRoomAmount * DeclarationTableInfo.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentHeader() {
            _headerTable.Rows[0][0] = "Сквозной номер квартиры";
            _headerTable.Rows[0][1] = "Назначение";
            _headerTable.Rows[0][2] = "Этаж расположения";
            _headerTable.Rows[0][3] = "Номер подъезда";
            _headerTable.Rows[0][4] = "Номер корпуса";
            _headerTable.Rows[0][5] = "Номер на площадке";
            _headerTable.Rows[0][6] = "Общая площадь без пониж. коэффициента, м2";
            _headerTable.Rows[0][7] = "Общая площадь с пониж. коэффициентом, м2";
            _headerTable.Rows[0][8] = "Общая жилая площадь, м2";
            _headerTable.Rows[0][9] = "Количество комнат";
            _headerTable.Rows[0][10] = "ИД Объекта";
            _headerTable.Rows[0][11] = "Площадь квартиры без летних помещений, м2";
            _headerTable.Rows[0][12] = "Высота потолка, м";

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
            int columnNumber = DeclarationTableInfo.InfoWidth;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.SummerRoomCells] = "№ Пом.";
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.SummerRoomCells + 1] = "Наименование на планировке";
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.SummerRoomCells + 2] = $"{priority.Name}_{k + 1}, площадь без коэф.";
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.SummerRoomCells + 3] = $"{priority.Name}_{k + 1}, площадь с коэф.";
                    }
                    columnNumber += priority.MaxRoomAmount * DeclarationTableInfo.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.MainRoomCells] = "№ Пом.";
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.MainRoomCells + 1] = "Наименование на планировке";
                        _headerTable.Rows[0][columnNumber + k * DeclarationTableInfo.MainRoomCells + 2] = $"{priority.Name}_{k + 1}";
                    }
                    columnNumber += priority.MaxRoomAmount * DeclarationTableInfo.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentsInfo() {
            int rowNumber = 0;

            foreach(Apartment apartment in _tableInfo.Apartments) {
                _mainTable.Rows[rowNumber][0] = apartment.FullNumber;
                _mainTable.Rows[rowNumber][1] = apartment.Department;
                _mainTable.Rows[rowNumber][2] = apartment.Level;
                _mainTable.Rows[rowNumber][3] = apartment.Section;
                _mainTable.Rows[rowNumber][4] = apartment.Building;
                _mainTable.Rows[rowNumber][5] = apartment.Number;
                _mainTable.Rows[rowNumber][6] = apartment.AreaMain;
                _mainTable.Rows[rowNumber][7] = apartment.AreaCoef;
                _mainTable.Rows[rowNumber][8] = apartment.AreaLiving;
                _mainTable.Rows[rowNumber][9] = apartment.RoomsAmount;
                _mainTable.Rows[rowNumber][10] = _settings.ProjectName;
                _mainTable.Rows[rowNumber][11] = apartment.AreaNonSummer;
                _mainTable.Rows[rowNumber][12] = apartment.RoomsHeight;

                int columnNumber = DeclarationTableInfo.InfoWidth;

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
            int rowNumber = 0;
            int columnNumber = _tableInfo.UtpStart;

            foreach(Apartment apartment in _tableInfo.Apartments) {
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
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.SummerRoomCells]
                        = rooms[k].Number;
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.SummerRoomCells + 1]
                        = rooms[k].DeclarationName;
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.SummerRoomCells + 2]
                        = rooms[k].Area;
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.SummerRoomCells + 3]
                        = rooms[k].AreaCoef;
                }
            }

            startColumn += priority.MaxRoomAmount * DeclarationTableInfo.SummerRoomCells;
            return startColumn;
        }

        private int FillMainRoomsByPriority(RoomPriority priority,
                                            IReadOnlyList<RoomElement> rooms,
                                            int rowNumber,
                                            int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.MainRoomCells]
                        = rooms[k].Number;
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.MainRoomCells + 1]
                        = rooms[k].DeclarationName;
                    _mainTable.Rows[rowNumber][startColumn + k * DeclarationTableInfo.MainRoomCells + 2]
                        = rooms[k].Area;
                }
            }

            startColumn += priority.MaxRoomAmount * DeclarationTableInfo.MainRoomCells;
            return startColumn;
        }

    }
}
