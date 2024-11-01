using Autodesk.Revit.DB;

namespace RevitScheduleImport.Services {
    public class LengthConverter {
        /// <summary>
        /// Коэффициент перевода единиц ширины столбца Excel в мм. 
        /// Получен эмпирическим путем из размеров ячеек при печати листа Excel.
        /// </summary>
        private const double _excelWidthToMm = 2.160651;

        /// <summary>
        /// Коэффициент перевода единиц высоты строки Excel в мм. 
        /// Получен эмпирическим путем из размеров ячеек при печати листа Excel.
        /// </summary>
        private const double _excelHeightToMm = 0.344086;
        private readonly double _footInMm;

        /// <summary>
        /// Коэффициент, на который надо умножить количество пунктов высоты текста из Excel,<br/>
        /// чтобы получить количество пунктов в Revit,<br/>
        /// чтобы по итогу значение высоты текста в мм было одинаковым.<br/>
        /// Получен эмпирическим путем из размеров текста при печати из Excel и Revit.
        /// </summary>
        private const double _excelPtToRvtPt = 0.842519685;

        public LengthConverter() {
            _footInMm = ConvertFromInternal(1);
        }


        public double ConvertFromInternal(double feetValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(feetValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
#endif
        }

        public double ConvertToInternal(double mmValue) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(mmValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Конвертирует ширину колонки Excel в единицы длины Revit (футы)
        /// </summary>
        /// <param name="excelColumnWidth">Ширина колонки в единицах Excel</param>
        /// <returns>Размер во внутренних единицах Revit (футах)</returns>
        public double ConvertExcelColWidthToInternal(double excelColumnWidth) {
            // коэффициенты перевода ширины столбца из единиц Excel в мм в документации явно не прописаны
            // https://github.com/ClosedXML/ClosedXML/wiki/Cell-Dimensions#width-1
            // поэтому берем коэффициент перевода, полученный из размеров ячеек при печати листа Excel
            return excelColumnWidth * _excelWidthToMm / _footInMm;
        }

        /// <summary>
        /// Конвертирует высоты строки Excel в единицы длины Revit (футы)
        /// </summary>
        /// <param name="excelRowHeight">Высота строки в единицах Excel</param>
        /// <returns>Размер во внутренних единицах Revit (футах)</returns>
        public double ConvertExcelRowHeightToInternal(double excelRowHeight) {
            // коэффициенты перевода высоты строки из единиц Excel в мм в документации явно не прописаны
            // https://github.com/ClosedXML/ClosedXML/wiki/Cell-Dimensions#height
            // поэтому берем коэффициент перевода, полученный из размеров ячеек при печати листа Excel
            return excelRowHeight * _excelHeightToMm / _footInMm;
        }

        /// <summary>
        /// Конвертирует высоту текста в пунктах Excel в высоту текста в единицах Revit
        /// </summary>
        /// <param name="excelFontSize">Высота текста в пунктах в Excel</param>
        /// <returns>Высота текста в единицах Revit</returns>
        public double ConvertExcelFontSizeToInternal(double excelFontSize) {
            return excelFontSize * _excelPtToRvtPt;
        }
    }
}
