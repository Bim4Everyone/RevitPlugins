namespace RevitDeclarations.Models;
internal interface ITableExporter {
    void Export(string path, IDeclarationDataTable declarationTable);
}
