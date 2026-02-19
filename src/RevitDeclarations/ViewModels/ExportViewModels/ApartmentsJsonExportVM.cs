using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class ApartmentsJsonExportVM : ExportViewModel {
    public ApartmentsJsonExportVM(string name, Guid id, DeclarationSettings settings, IMessageBoxService messageBoxService)
        : base(name, id, settings, messageBoxService) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var apartments = roomGroups.Cast<Apartment>();
        var exporter = new JsonExporter<Apartment>();
        exporter.Export(path, apartments);
        _messageBoxService.Show($"Файл {Name} создан", "Декларации");
    }
}
