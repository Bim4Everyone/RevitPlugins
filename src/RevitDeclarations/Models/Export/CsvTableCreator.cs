using System.Data;
using System.Linq;
using System.Text;

namespace RevitDeclarations.Models {
    internal class CsvTableCreator {
        private readonly ExcelTableData _excelTableData;
        private readonly DeclarationSettings _settings;
        private readonly DataTable _table;

        public CsvTableCreator(ExcelTableData tableData, DeclarationSettings settings) {
            _excelTableData = tableData;
            _settings = settings;

            _table = new DeclarationDataTable(_excelTableData, _settings).DataTable;
        }

        public string Create() {
            StringBuilder strBuilder = new StringBuilder();

            foreach(DataRow dataRow in _table.Rows) {
                string[] fields = dataRow
                    .ItemArray
                    .Select(field => field.ToString())
                    .ToArray();

                strBuilder.AppendLine(string.Join("\t", fields));
            }

            return strBuilder.ToString();
        }
    }
}
