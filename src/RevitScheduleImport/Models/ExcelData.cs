using System;

using ClosedXML.Excel;

namespace RevitScheduleImport.Models {
    public class ExcelData : IDisposable {
        private readonly IXLWorkbook _workbook;

        public ExcelData(IXLWorkbook workbook) {
            _workbook = workbook ?? throw new ArgumentNullException(nameof(workbook));
        }


        public IXLWorkbook Workbook => _workbook;


        public void Dispose() {
            _workbook?.Dispose();
        }
    }
}
