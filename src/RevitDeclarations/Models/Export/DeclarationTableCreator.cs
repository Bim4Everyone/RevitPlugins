using System.Collections.Generic;
using System.Linq;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models {
    internal class DeclarationTableCreator {
        private readonly DeclarationTableData _tableData;
        private readonly DeclarationSettings _settings;

        public DeclarationTableCreator(DeclarationTableData tableData, DeclarationSettings settings) {
            _tableData = tableData;
            _settings = settings;
        }

        public void Create(Worksheet workSheet) {
            Range range = workSheet.Cells;

            FillTableApartmentHeader(range);
            FillTableRoomsHeader(range);
            FillTableApartmentsInfo(range);

            if(_settings.LoadUtp) {
                FillTableUtpInfo(range);
            }

            SetGraphicSettings(workSheet);
        }

        private void FillTableApartmentHeader(Range range) {
            range[1, 1] = "Сквозной номер квартиры";
            range[1, 2] = "Назначение";
            range[1, 3] = "Этаж расположения";
            range[1, 4] = "Номер подъезда";
            range[1, 5] = "Номер корпуса";
            range[1, 6] = "Номер на площадке";
            range[1, 7] = "Общая площадь без пониж. коэффициента, м2";
            range[1, 8] = "Общая площадь с пониж. коэффициентом, м2";
            range[1, 9] = "Жилая площадь, м2";
            range[1, 10] = "Количество комнат";
            range[1, 11] = "ИД Объекта";
            range[1, 12] = "Площадь квартиры без летних помещений, м2";
            range[1, 13] = "Высота потолка, м";

            if(_settings.LoadUtp) {
                range[1, _tableData.UtpStart + 1] = "Две ванны";
                range[1, _tableData.UtpStart + 2] = "Хайфлет";
                range[1, _tableData.UtpStart + 3] = "Лоджия/ балкон";
                range[1, _tableData.UtpStart + 4] = "Доп. летние помещения";
                range[1, _tableData.UtpStart + 5] = "Терраса";
                range[1, _tableData.UtpStart + 6] = "Мастер-спальня";
                range[1, _tableData.UtpStart + 7] = "Гардеробная";
                range[1, _tableData.UtpStart + 8] = "Постирочная";
                range[1, _tableData.UtpStart + 9] = "Увеличенная площадь балкона/ лоджии";
            }
        }

        private void FillTableRoomsHeader(Range range) {
            int columnNumber = _tableData.InfoWidth + 1;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        range[1, columnNumber + k * _tableData.SummerRoomCells] = "№ Пом.";
                        range[1, columnNumber + k * _tableData.SummerRoomCells + 1] = "Наименование на планировке";
                        range[1, columnNumber + k * _tableData.SummerRoomCells + 2] = $"{priority.Name}_{k + 1}, площадь без коэф.";
                        range[1, columnNumber + k * _tableData.SummerRoomCells + 3] = $"{priority.Name}_{k + 1}, площадь с коэф.";
                    }
                    columnNumber += priority.MaxRoomAmount * _tableData.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        range[1, columnNumber + k * _tableData.MainRoomCells] = "№ Пом.";
                        range[1, columnNumber + k * _tableData.MainRoomCells + 1] = "Наименование на планировке";
                        range[1, columnNumber + k * _tableData.MainRoomCells + 2] = $"{priority.Name}_{k + 1}";
                    }
                    columnNumber += priority.MaxRoomAmount * _tableData.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentsInfo(Range range) {
            int rowNumber = 2;

            foreach(Apartment apartment in _tableData.Apartments) {
                range[rowNumber, 1] = apartment.FullNumber;
                range[rowNumber, 2] = apartment.Department;
                range[rowNumber, 3] = apartment.Level;
                range[rowNumber, 4] = apartment.Section;
                range[rowNumber, 5] = apartment.Building;
                range[rowNumber, 6] = apartment.Number;
                range[rowNumber, 7] = apartment.AreaMain;
                range[rowNumber, 8] = apartment.AreaCoef;
                range[rowNumber, 9] = apartment.AreaLiving;
                range[rowNumber, 10] = apartment.RoomsAmount;
                range[rowNumber, 11] = _settings.ProjectName;
                range[rowNumber, 12] = apartment.AreaNonSummer;
                range[rowNumber, 13] = apartment.RoomsHeight;

                int columnNumber = _tableData.InfoWidth + 1;

                foreach(RoomPriority priority in _settings.UsedPriorities) {
                    List<RoomElement> rooms = apartment.GetRoomsByPrior(priority);

                    if(priority.IsSummer) {
                        columnNumber = FillSummerRoomsByPriority(range, priority, rooms, rowNumber, columnNumber);
                    } else {
                        columnNumber = FillMainRoomsByPriority(range, priority, rooms, rowNumber, columnNumber);
                    }
                }

                rowNumber++;
            }
        }

        private void FillTableUtpInfo(Range range) {
            int rowNumber = 2;
            int columnNumber = _tableData.UtpStart;

            foreach(Apartment apartment in _tableData.Apartments) {
                range[rowNumber, columnNumber + 1] = apartment.UtpTwoBaths;
                range[rowNumber, columnNumber + 2] = apartment.UtpHighflat;
                range[rowNumber, columnNumber + 3] = apartment.UtpBalcony;
                range[rowNumber, columnNumber + 4] = apartment.UtpExtraSummerRooms;
                range[rowNumber, columnNumber + 5] = apartment.UtpTerrace;
                range[rowNumber, columnNumber + 6] = apartment.UtpMasterBedroom;
                range[rowNumber, columnNumber + 7] = apartment.UtpPantry;
                range[rowNumber, columnNumber + 8] = apartment.UtpLaundry;
                range[rowNumber, columnNumber + 9] = apartment.UtpExtraBalconyArea;

                rowNumber++;
            }
        }

        private int FillSummerRoomsByPriority(Range range,
                                       RoomPriority priority,
                                       List<RoomElement> rooms,
                                       int rowNumber,
                                       int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    range[rowNumber, startColumn + k * _tableData.SummerRoomCells]
                        = rooms[k].Number;
                    range[rowNumber, startColumn + k * _tableData.SummerRoomCells + 1]
                        = rooms[k].DeclarationName;
                    range[rowNumber, startColumn + k * _tableData.SummerRoomCells + 2]
                        = rooms[k].GetAreaParamValue(_settings.RoomAreaParam, _settings.Accuracy);
                    range[rowNumber, startColumn + k * _tableData.SummerRoomCells + 3]
                        = rooms[k].GetAreaParamValue(_settings.RoomAreaCoefParam, _settings.Accuracy);
                }
            }

            startColumn += priority.MaxRoomAmount * _tableData.SummerRoomCells;
            return startColumn;
        }

        private int FillMainRoomsByPriority(Range range,
                                      RoomPriority priority,
                                      List<RoomElement> rooms,
                                      int rowNumber,
                                      int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    range[rowNumber, startColumn + k * _tableData.MainRoomCells]
                        = rooms[k].Number;
                    range[rowNumber, startColumn + k * _tableData.MainRoomCells + 1]
                        = rooms[k].DeclarationName;
                    range[rowNumber, startColumn + k * _tableData.MainRoomCells + 2]
                        = rooms[k].GetAreaParamValue(_settings.RoomAreaParam, _settings.Accuracy);
                }
            }

            startColumn += priority.MaxRoomAmount * _tableData.MainRoomCells;
            return startColumn;
        }

        private void SetGraphicSettings(Worksheet workSheet) {
            workSheet.StandardWidth = 12;
            Range range = workSheet.Rows[1];

            range.RowHeight = 60;
            range.WrapText = true;

            Font font = range.Font;
            font.Bold = true;

            workSheet.Columns[1].NumberFormat = "@";

            Range firstCell = workSheet.Cells[1, 1];
            Range lastCell = workSheet.Cells[_tableData.Apartments.Count + 1, _tableData.FullTableWidth];

            workSheet.Range[firstCell, lastCell].Borders.ColorIndex = 0;
            workSheet.Range[firstCell, lastCell].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            workSheet.Range[firstCell, lastCell].VerticalAlignment = XlVAlign.xlVAlignCenter;

            for(int i = 1; i <= _tableData.FullTableWidth; i++) {
                if(i <= _tableData.InfoWidth) {
                    workSheet.Columns[i].ColumnWidth = 15.5;
                    workSheet.Cells[1, i].Interior.Color = System.Drawing.Color.FromArgb(221, 235, 247);
                } else if(i > _tableData.InfoWidth && i <= _tableData.SummerRoomsStart) {
                    workSheet.Columns[i].ColumnWidth = 10;
                    workSheet.Cells[1, i].Interior.Color = System.Drawing.Color.FromArgb(248, 203, 173);

                    int checkColumnNumber = (i - _tableData.InfoWidth) % 3;
                    if(checkColumnNumber == 0) {
                        workSheet.Columns[i - 2].NumberFormat = "@";
                        workSheet.Columns[i - 1].ColumnWidth = 17;
                        workSheet.Columns[i].NumberFormat = "0,0";
                    }
                } else if(i > _tableData.SummerRoomsStart && i <= _tableData.OtherRoomsStart) {
                    workSheet.Columns[i].ColumnWidth = 10;
                    workSheet.Cells[1, i].Interior.Color = System.Drawing.Color.FromArgb(217, 235, 205);

                    int checkColumnNumber = (i - _tableData.SummerRoomsStart) % 4;
                    if(checkColumnNumber == 0) {
                        workSheet.Columns[i - 3].NumberFormat = "@";
                        workSheet.Columns[i - 2].ColumnWidth = 17;
                        workSheet.Columns[i - 1].NumberFormat = "0,0";
                        workSheet.Columns[i].NumberFormat = "0,0";
                    }
                } else if(i > _tableData.OtherRoomsStart && i <= _tableData.UtpStart) {
                    workSheet.Columns[i].ColumnWidth = 10;
                    workSheet.Cells[1, i].Interior.Color = System.Drawing.Color.FromArgb(237, 237, 237);

                    int checkColumnNumber = (i - _tableData.OtherRoomsStart) % 3;
                    if(checkColumnNumber == 0) {
                        workSheet.Columns[i - 2].NumberFormat = "@";
                        workSheet.Columns[i - 1].ColumnWidth = 17;
                        workSheet.Columns[i].NumberFormat = "0,0";
                    }
                } else {
                    workSheet.Columns[i].ColumnWidth = 12;
                    workSheet.Cells[1, i].Interior.Color = System.Drawing.Color.FromArgb(226, 207, 245);
                }
            }
        }
    }
}
