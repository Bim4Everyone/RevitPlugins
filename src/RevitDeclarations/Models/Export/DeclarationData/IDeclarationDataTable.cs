using System.Collections.Generic;
using System.Data;

namespace RevitDeclarations.Models;
internal interface IDeclarationDataTable {
    string Name { get; }
    DataTable HeaderDataTable { get; }
    DataTable MainDataTable { get; }
    List<IDeclarationDataTable> SubTables { get; }

    ITableInfo TableInfo { get; }
}
