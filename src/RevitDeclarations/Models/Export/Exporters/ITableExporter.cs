namespace RevitDeclarations.Models {
    internal interface ITableExporter {
        void Export(string path, DeclarationDataTable declarationTable);
    }
}
