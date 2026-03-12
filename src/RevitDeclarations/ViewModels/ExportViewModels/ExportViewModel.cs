using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal abstract class ExportViewModel {
    protected readonly DeclarationSettings _settings;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageBoxService _messageBoxService;

    protected const string _declarationApartsName = "15.2 Жилые помещения";
    protected const string _declarationCommercialName = "15.3 Нежилые помещения";
    protected const string _declarationPublicAreasName = "16.1 МОП";

    protected ExportViewModel(string name, 
                              Guid id, 
                              DeclarationSettings settings,
                              ILocalizationService localizationService,
                              IMessageBoxService messageBoxService) {
        Id = id;
        Name = name;
        _settings = settings;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
    }

    public string Name { get; }
    public Guid Id { get; }

    public void ExportTable<T>(string path, IDeclarationDataTable table) where T : ITableExporter, new() {
        var exporter = new T();
        exporter.Export(path, table, _localizationService, _messageBoxService);
        _messageBoxService.Show(
            _localizationService.GetLocalizedString("MessageBox.DeclFileCreated", Name),
            _localizationService.GetLocalizedString("MainWindow.Title"));
    }

    public abstract void Export(string path, IEnumerable<RoomGroup> roomGroups);
}
