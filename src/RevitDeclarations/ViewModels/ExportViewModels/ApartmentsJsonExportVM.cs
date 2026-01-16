using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels;
internal class ApartmentsJsonExportVM : ExportViewModel {
    public ApartmentsJsonExportVM(string name, Guid id, DeclarationSettings settings)
        : base(name, id, settings) {
    }

    public override void Export(string path, IEnumerable<RoomGroup> roomGroups) {
        var apartments = roomGroups.Cast<Apartment>();
        var exporter = new JsonExporter<Apartment>();
        exporter.Export(path, apartments);
        TaskDialog.Show("Декларации", $"Файл {Name} создан");
    }
}
