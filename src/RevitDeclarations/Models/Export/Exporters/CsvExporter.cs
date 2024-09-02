using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace RevitDeclarations.Models {
    internal class CsvExporter : ITableExporter {
        public void Export(string path, DeclarationDataTable table) {
            path = Path.ChangeExtension(path, "scv");

            string strData = ConvertDataTableToString(table.DataTable);

            using(StreamWriter file = File.CreateText(path)) {
                file.Write(strData);
            }
        }

        private string ConvertDataTableToString(DataTable table) {
            StringBuilder strBuilder = new StringBuilder();

            foreach(DataRow dataRow in table.Rows) {
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
