using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class JsonExportViewModel : ExportViewModel {
        public JsonExportViewModel(string name, Guid id, DeclarationSettings settings) 
            : base(name, id, settings) {
        }

        public override void Export(string path, IEnumerable<RoomGroup> apartments) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments.Cast<Apartment>());
            TaskDialog.Show("Декларации", $"Файл {Name} создан");
        }
    }
}
