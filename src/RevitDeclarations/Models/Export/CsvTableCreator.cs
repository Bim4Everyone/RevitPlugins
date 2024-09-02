using System.Data;
using System.Linq;
using System.Text;

namespace RevitDeclarations.Models {
    internal class CsvTableCreator {
        public string Create(DataTable table) {
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
