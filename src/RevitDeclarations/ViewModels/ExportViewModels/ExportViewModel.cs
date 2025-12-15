using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal abstract class ExportViewModel {
    protected readonly DeclarationSettings _settings;

    protected ExportViewModel(string name, Guid id, DeclarationSettings settings) {
        Id = id;
        Name = name;
        _settings = settings;
    }

    public string Name { get; }
    public Guid Id { get; }

    public void ExportTable<T>(string path, IDeclarationDataTable table) where T : ITableExporter, new() {
        var exporter = new T();
        exporter.Export(path, table);
        TaskDialog.Show("Декларации", $"Файл {Name} создан");
    }

    public abstract void Export(string path, IEnumerable<RoomGroup> roomGroups);
}
