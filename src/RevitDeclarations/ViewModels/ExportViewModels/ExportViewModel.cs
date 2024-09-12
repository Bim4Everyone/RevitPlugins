using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal abstract class ExportViewModel {
        private readonly string _name;
        private protected readonly DeclarationSettings _settings;

        protected ExportViewModel(string name, DeclarationSettings settings) {
            _name = name;
            _settings = settings;
        }

        public string Name => _name;

        public void ExportTable<T>(string path, DeclarationDataTable table) where T : ITableExporter, new() {
            T exporter = new T();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", $"Файл {Name} создан");
        }

        public virtual void Export(string path, IEnumerable<Apartment> apartments) { 
        }
    }
}
