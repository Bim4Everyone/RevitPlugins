using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal abstract class ExportViewModel {
    protected readonly DeclarationSettings _settings;
    protected readonly IMessageBoxService _messageBoxService;

    protected ExportViewModel(string name, Guid id, DeclarationSettings settings, IMessageBoxService messageBoxService) {
        Id = id;
        Name = name;
        _settings = settings;
        _messageBoxService = messageBoxService;
    }

    public string Name { get; }
    public Guid Id { get; }

    public void ExportTable<T>(string path, IDeclarationDataTable table) where T : ITableExporter, new() {
        var exporter = new T();
        exporter.Export(path, table, _messageBoxService);
        _messageBoxService.Show($"Файл {Name} создан", "Декларации");
    }

    public abstract void Export(string path, IEnumerable<RoomGroup> roomGroups);
}
