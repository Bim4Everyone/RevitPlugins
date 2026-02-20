using dosymep.SimpleServices;

namespace RevitDeclarations.Models;
internal interface ITableExporter {
    void Export(string path, 
                IDeclarationDataTable declarationTable,
                ILocalizationService localizationService,
                IMessageBoxService messageBoxService);
}
