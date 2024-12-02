using System.IO;

using ClosedXML.Excel;

using RevitScheduleImport.Models;

namespace RevitScheduleImport.Services {
    public class ExcelReader {
        public ExcelData ReadExcel(string path) {
            if(!File.Exists(path)) {
                throw new FileNotFoundException("Excel файл не найден.", path);
            }

            using(Stream stream = File.OpenRead(path)) {
                return new ExcelData(new XLWorkbook(stream));
            }
        }
    }
}
