using System.Data;
using System.IO;
using System.Linq;
using System.Text;

using dosymep.SimpleServices;

namespace RevitDeclarations.Models;
internal class CsvExporter : ITableExporter {
    public void Export(string path, 
                       IDeclarationDataTable table,
                       ILocalizationService localizationService,
                       IMessageBoxService messageBoxService) {
        string fullPath = $"{path}.csv";

        string strData = ConvertDataTableToString(table);

        using(var file = File.CreateText(fullPath)) {
            file.Write(strData);
        }

        if(table.SubTables.Any()) {
            int tableNumber = 1;
            foreach(var subTable in table.SubTables) {
                path = $"{path}-{tableNumber}";
                Export(path, subTable, localizationService, messageBoxService);
                tableNumber++;
            }
        }
    }

    private string ConvertDataTableToString(IDeclarationDataTable table) {
        var strBuilder = new StringBuilder();

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
