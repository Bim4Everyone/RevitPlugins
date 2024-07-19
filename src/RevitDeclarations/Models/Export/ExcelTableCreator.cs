using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Microsoft.Office.Interop.Excel;

namespace RevitDeclarations.Models {
    internal class ExcelTableCreator {
        private readonly Color _apartInfoColor = Color.FromArgb(221, 235, 247);
        private readonly Color _mainRoomsColor = Color.FromArgb(248, 203, 173);
        private readonly Color _summerRoomsColor = Color.FromArgb(217, 235, 205);
        private readonly Color _nonConfigRoomsColor = Color.FromArgb(237, 237, 237);
        private readonly Color _utpColor = Color.FromArgb(226, 207, 245);

        private readonly ExcelTableData _excelTableData;
        private readonly DeclarationSettings _settings;

        public ExcelTableCreator(ExcelTableData tableData, DeclarationSettings settings) {
            _excelTableData = tableData;
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
            range[1, 9] = "Общая жилая площадь, м2";
            range[1, 10] = "Количество комнат";
            range[1, 11] = "ИД Объекта";
            range[1, 12] = "Площадь квартиры без летних помещений, м2";
            range[1, 13] = "Высота потолка, м";

            if(_settings.LoadUtp) {
                range[1, _excelTableData.UtpStart + 1] = "Две ванны";
                range[1, _excelTableData.UtpStart + 2] = "Хайфлет";
                range[1, _excelTableData.UtpStart + 3] = "Лоджия/ балкон";
                range[1, _excelTableData.UtpStart + 4] = "Доп. летние помещения";
                range[1, _excelTableData.UtpStart + 5] = "Терраса";
                range[1, _excelTableData.UtpStart + 6] = "Мастер-спальня";
                range[1, _excelTableData.UtpStart + 7] = "Гардеробная";
                range[1, _excelTableData.UtpStart + 8] = "Постирочная";
                range[1, _excelTableData.UtpStart + 9] = "Увеличенная площадь балкона/ лоджии";
            }
        }

        private void FillTableRoomsHeader(Range range) {
            int columnNumber = ExcelTableData.InfoWidth + 1;

            foreach(RoomPriority priority in _settings.UsedPriorities) {
                if(priority.IsSummer) {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        range[1, columnNumber + k * ExcelTableData.SummerRoomCells] = "№ Пом.";
                        range[1, columnNumber + k * ExcelTableData.SummerRoomCells + 1] = "Наименование на планировке";
                        range[1, columnNumber + k * ExcelTableData.SummerRoomCells + 2] = $"{priority.Name}_{k + 1}, площадь без коэф.";
                        range[1, columnNumber + k * ExcelTableData.SummerRoomCells + 3] = $"{priority.Name}_{k + 1}, площадь с коэф.";
                    }
                    columnNumber += priority.MaxRoomAmount * ExcelTableData.SummerRoomCells;
                } else {
                    for(int k = 0; k < priority.MaxRoomAmount; k++) {
                        range[1, columnNumber + k * ExcelTableData.MainRoomCells] = "№ Пом.";
                        range[1, columnNumber + k * ExcelTableData.MainRoomCells + 1] = "Наименование на планировке";
                        range[1, columnNumber + k * ExcelTableData.MainRoomCells + 2] = $"{priority.Name}_{k + 1}";
                    }
                    columnNumber += priority.MaxRoomAmount * ExcelTableData.MainRoomCells;
                }
            }
        }

        private void FillTableApartmentsInfo(Range range) {
            int rowNumber = 2;

            foreach(Apartment apartment in _excelTableData.Apartments) {
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

                int columnNumber = ExcelTableData.InfoWidth + 1;

                foreach(RoomPriority priority in _settings.UsedPriorities) {
                    IReadOnlyList<RoomElement> rooms = apartment.GetRoomsByPrior(priority);

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
            int columnNumber = _excelTableData.UtpStart;

            foreach(Apartment apartment in _excelTableData.Apartments) {
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
                                       IReadOnlyList<RoomElement> rooms,
                                       int rowNumber,
                                       int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    range[rowNumber, startColumn + k * ExcelTableData.SummerRoomCells]
                        = rooms[k].Number;
                    range[rowNumber, startColumn + k * ExcelTableData.SummerRoomCells + 1]
                        = rooms[k].DeclarationName;
                    range[rowNumber, startColumn + k * ExcelTableData.SummerRoomCells + 2]
                        = rooms[k].Area;
                    range[rowNumber, startColumn + k * ExcelTableData.SummerRoomCells + 3]
                        = rooms[k].AreaCoef;
                }
            }

            startColumn += priority.MaxRoomAmount * ExcelTableData.SummerRoomCells;
            return startColumn;
        }

        private int FillMainRoomsByPriority(Range range,
                                      RoomPriority priority,
                                      IReadOnlyList<RoomElement> rooms,
                                      int rowNumber,
                                      int startColumn) {
            for(int k = 0; k < priority.MaxRoomAmount; k++) {
                if(rooms.ElementAtOrDefault(k) != null) {
                    range[rowNumber, startColumn + k * ExcelTableData.MainRoomCells]
                        = rooms[k].Number;
                    range[rowNumber, startColumn + k * ExcelTableData.MainRoomCells + 1]
                        = rooms[k].DeclarationName;
                    range[rowNumber, startColumn + k * ExcelTableData.MainRoomCells + 2]
                        = rooms[k].Area;
                }
            }

            startColumn += priority.MaxRoomAmount * ExcelTableData.MainRoomCells;
            return startColumn;
        }

        private void SetGraphicSettings(Worksheet workSheet) {
            workSheet.StandardWidth = 12;
            Range range = (Range) workSheet.Rows[1];

            range.RowHeight = 60;
            range.WrapText = true;

            Microsoft.Office.Interop.Excel.Font font = range.Font;
            font.Bold = true;

            ((Range) workSheet.Columns[1]).NumberFormat = "@";

            Range firstCell = (Range) workSheet.Cells[1, 1];
            Range lastCell = (Range) workSheet.Cells[_excelTableData.Apartments.Count + 1, _excelTableData.FullTableWidth];

            workSheet.Range[firstCell, lastCell].Borders.ColorIndex = 0;
            workSheet.Range[firstCell, lastCell].HorizontalAlignment = XlHAlign.xlHAlignCenter;
            workSheet.Range[firstCell, lastCell].VerticalAlignment = XlVAlign.xlVAlignCenter;

            for(int i = 1; i <= _excelTableData.FullTableWidth; i++) {
                if(i <= ExcelTableData.InfoWidth) {
                    ((Range) workSheet.Columns[i]).ColumnWidth = 15.5;
                    ((Range) workSheet.Cells[1, i]).Interior.Color = _apartInfoColor;
                } else if(i > ExcelTableData.InfoWidth && i <= _excelTableData.SummerRoomsStart) {
                    ((Range)workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range)workSheet.Cells[1, i]).Interior.Color = _mainRoomsColor;

                    int checkColumnNumber = (i - ExcelTableData.InfoWidth) % 3;
                    if(checkColumnNumber == 0) {
                        ((Range)workSheet.Columns[i - 2]).NumberFormat = "@";
                        ((Range)workSheet.Columns[i - 1]).ColumnWidth = 17;
                        ((Range)workSheet.Columns[i]).NumberFormat = "0,0";
                    }
                } else if(i > _excelTableData.SummerRoomsStart && i <= _excelTableData.OtherRoomsStart) {
                    ((Range)workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range)workSheet.Cells[1, i]).Interior.Color = _summerRoomsColor;

                    int checkColumnNumber = (i - _excelTableData.SummerRoomsStart) % 4;
                    if(checkColumnNumber == 0) {
                        ((Range)workSheet.Columns[i - 3]).NumberFormat = "@";
                        ((Range)workSheet.Columns[i - 2]).ColumnWidth = 17;
                        ((Range)workSheet.Columns[i - 1]).NumberFormat = "0,0";
                        ((Range)workSheet.Columns[i]).NumberFormat = "0,0";
                    }
                } else if(i > _excelTableData.OtherRoomsStart && i <= _excelTableData.UtpStart) {
                    ((Range)workSheet.Columns[i]).ColumnWidth = 10;
                    ((Range)workSheet.Cells[1, i]).Interior.Color = _nonConfigRoomsColor;

                    int checkColumnNumber = (i - _excelTableData.OtherRoomsStart) % 3;
                    if(checkColumnNumber == 0) {
                        ((Range)workSheet.Columns[i - 2]).NumberFormat = "@";
                        ((Range)workSheet.Columns[i - 1]).ColumnWidth = 17;
                        ((Range)workSheet.Columns[i]).NumberFormat = "0,0";
                    }
                } else {
                    ((Range)workSheet.Columns[i]).ColumnWidth = 12;
                    ((Range)workSheet.Cells[1, i]).Interior.Color = _utpColor;
                }
            }
        }
    }
}
