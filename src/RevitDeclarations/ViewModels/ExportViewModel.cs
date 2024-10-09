using System;
using System.Collections.Generic;

using Autodesk.Revit.UI;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal abstract class ExportViewModel {
        private readonly string _name;
        private readonly Guid _id;
        private protected readonly DeclarationSettings _settings;

        protected ExportViewModel(string name, Guid id, DeclarationSettings settings) {
            _id = id;
            _name = name;
            _settings = settings;
        }

        public string Name => _name;
        public Guid Id => _id;

        public void ExportTable<T>(string path, IDeclarationDataTable table) where T : ITableExporter, new() {
            T exporter = new T();
            exporter.Export(path, table);
            TaskDialog.Show("Декларации", $"Файл {Name} создан");
        }

        public virtual void Export(string path, IEnumerable<RoomGroup> apartments) { 
        }
    }
}