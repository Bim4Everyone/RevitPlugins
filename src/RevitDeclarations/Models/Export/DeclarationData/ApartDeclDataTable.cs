using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RevitDeclarations.Models {
    internal class ApartDeclDataTable : IDeclarationDataTable {
        private readonly ApartDeclTableInfo _tableInfo;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _mainTable;
        private readonly DataTable _headerTable;

        public ApartDeclDataTable(ApartDeclTableInfo tableData, DeclarationSettings settings) {
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
        public ApartDeclTableInfo TableInfo => _tableInfo;

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
            _mainTable.Columns[6].DataType = typeof(double);
            _mainTable.Columns[7].DataType = typeof(double);
            _mainTable.Columns[8].DataType = typeof(double);
            _mainTable.Columns[11].DataType = typeof(double);
            _mainTable.Columns[12].DataType = typeof(double);

            int columnNumber = ApartDeclTableInfo.InfoWidth;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * ApartDeclTableInfo.SummerRoomCells;
                        _mainTable.Columns[columnIndex + 2].DataType = typeof(double);
                        _mainTable.Columns[columnIndex + 3].DataType = typeof(double);
                    }
                    columnNumber += priority.MaxRoomAmount * ApartDeclTableInfo.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * ApartDeclTableInfo.MainRoomCells;
                        _mainTable.Columns[columnIndex + 2].DataType = typeof(double);
                    }
                    columnNumber += priority.MaxRoomAmount * ApartDeclTableInfo.MainRoomCells;
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
            int columnNumber = ApartDeclTableInfo.InfoWidth;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * ApartDeclTableInfo.SummerRoomCells;
                        _headerTable.Rows[0][columnIndex] = "№ Пом.";
                        _headerTable.Rows[0][columnIndex + 1] = "Наименование на планировке";
                        _headerTable.Rows[0][columnIndex + 2] = $"{priority.Name}_{k + 1}, площадь без коэф.";
                        _headerTable.Rows[0][columnIndex + 3] = $"{priority.Name}_{k + 1}, площадь с коэф.";
                    }
                    columnNumber += priority.MaxRoomAmount * ApartDeclTableInfo.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        int columnIndex = columnNumber + k * ApartDeclTableInfo.MainRoomCells;
                        _headerTable.Rows[0][columnIndex] = "№ Пом.";
                        _headerTable.Rows[0][columnIndex + 1] = "Наименование на планировке";
                        _headerTable.Rows[0][columnIndex + 2] = $"{priority.Name}_{k + 1}";
                    }
                    columnNumber += priority.MaxRoomAmount * ApartDeclTableInfo.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentsInfo() {
            int rowNumber = 0;

            foreach(Apartment apartment in _tableInfo.RoomGroups) {
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

                int columnNumber = ApartDeclTableInfo.InfoWidth;

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
                    int columnIndex = startColumn + k * ApartDeclTableInfo.SummerRoomCells;
                    _mainTable.Rows[rowNumber][columnIndex] = rooms[k].Number;
                    _mainTable.Rows[rowNumber][columnIndex + 1] = rooms[k].DeclarationName;
                    _mainTable.Rows[rowNumber][columnIndex + 2] = rooms[k].Area;
                    _mainTable.Rows[rowNumber][columnIndex + 3] = rooms[k].AreaCoef;
                }
            }

            startColumn += priority.MaxRoomAmount * ApartDeclTableInfo.SummerRoomCells;
            return startColumn;
        }

        private int FillMainRoomsByPriority(RoomPriority priority,
                                            IReadOnlyList<RoomElement> rooms,
                                            int rowNumber,
                                            int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    int columnIndex = startColumn + k * ApartDeclTableInfo.MainRoomCells;
                    _mainTable.Rows[rowNumber][columnIndex] = rooms[k].Number;
                    _mainTable.Rows[rowNumber][columnIndex + 1] = rooms[k].DeclarationName;
                    _mainTable.Rows[rowNumber][columnIndex + 2] = rooms[k].Area;
                }
            }

            startColumn += priority.MaxRoomAmount * ApartDeclTableInfo.MainRoomCells;
            return startColumn;
        }

    }
}
