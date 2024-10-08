using Autodesk.Revit.DB;

namespace RevitScheduleImport.Services {
    public class LengthConverter {
        /// <summary>
        /// Коэффициент перевода единиц ширины столбца Excel в мм
        /// </summary>
        private const double _excelWidthToMm = 2.160651;

        /// <summary>
        /// Коэффициент перевода единиц высоты строки Excel в мм
        /// </summary>
        private const double _excelHeightToMm = 0.344086;
        private readonly double _footInMm;

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
        /// <returns>Размер в внутренних единицах Revit (футах)</returns>
        public double ConvertExcelColWidthToInternal(double excelColumnWidth) {
            // fucking width
            // https://github.com/ClosedXML/ClosedXML/wiki/Cell-Dimensions#width-1
            return excelColumnWidth * _excelWidthToMm / _footInMm;
        }

        /// <summary>
        /// Конвертирует высоты строки Excel в единицы длины Revit (футы)
        /// </summary>
        /// <param name="excelColumnWidth">Высота строки в единицах Excel</param>
        /// <returns>Размер в внутренних единицах Revit (футах)</returns>
        public double ConvertExcelRowHeightToInternal(double excelRowHeight) {
            // fucking height
            // https://github.com/ClosedXML/ClosedXML/wiki/Cell-Dimensions#height
            return excelRowHeight * _excelHeightToMm / _footInMm;
        }
    }
}
