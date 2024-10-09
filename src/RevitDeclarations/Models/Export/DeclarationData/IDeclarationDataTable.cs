using System.Data;

namespace RevitDeclarations.Models {
    internal interface IDeclarationDataTable {
        DataTable HeaderDataTable { get; }
        DataTable MainDataTable { get; }

        ITableInfo TableInfo { get; }
    }
}
