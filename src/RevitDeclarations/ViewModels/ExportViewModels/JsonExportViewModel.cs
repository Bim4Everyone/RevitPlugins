using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class JsonExportViewModel : ExportViewModel {
        public JsonExportViewModel(string name, DeclarationSettings settings) 
            : base(name, settings) {
        }

        public override void Export(string path, IEnumerable<Apartment> apartments) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments);
            TaskDialog.Show("Декларации", $"Файл {Name} создан");
        }
    }
}
