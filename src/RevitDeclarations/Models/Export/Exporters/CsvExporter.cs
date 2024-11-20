using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RevitDeclarations.Models {
    internal class CsvExporter : ITableExporter {
        public void Export(string path, DeclarationDataTable table) {
            path = Path.ChangeExtension(path, "csv");

            string strData = ConvertDataTableToString(table);

            using(StreamWriter file = File.CreateText(path)) {
                file.Write(strData);
            }
        }

        private string ConvertDataTableToString(DeclarationDataTable table) {
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
