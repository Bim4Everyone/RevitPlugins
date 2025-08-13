using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RevitDeclarations.Models {
    internal class CsvExporter : ITableExporter {
        public void Export(string path, IDeclarationDataTable table) {
            string fullPath = $"{path}.csv";

            string strData = ConvertDataTableToString(table);

            using(StreamWriter file = File.CreateText(fullPath)) {
                file.Write(strData);
            }

            if(table.SubTables.Any()) {
                int tableNumber = 1;
                foreach(var subTable in table.SubTables) {
                    path = $"{path}-{tableNumber}";
                    Export(path, subTable);
                    tableNumber++;
                }
            }
        }

        private string ConvertDataTableToString(IDeclarationDataTable table) {
            StringBuilder strBuilder = new StringBuilder();

            string[] headerFields = table.HeaderDataTable.Rows[0]
                    .ItemArray
                    .Select(field => field.ToString())
                    .ToArray();
            strBuilder.AppendLine(string.Join("\t", headerFields));

            foreach(DataRow dataRow in table.MainDataTable.Rows) {
                string[] mainFields = dataRow
                    .ItemArray
                    .Select(field => field.ToString())
                    .ToArray();

                strBuilder.AppendLine(string.Join("\t", mainFields));
            }

            return strBuilder.ToString();
        }
    }
}
