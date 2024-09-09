using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal class JsonExportViewModel : ExportViewModel {
        public JsonExportViewModel(string name) : base(name) {
        }

        public override void Export(string path,
                                    IEnumerable<Apartment> apartments,
                                    DeclarationSettings settings) {
            JsonExporter<Apartment> exporter = new JsonExporter<Apartment>();
            exporter.Export(path, apartments);
            TaskDialog.Show("Декларации", "Файл JSON создан");
        }
    }
}
