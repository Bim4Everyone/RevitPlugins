using System.Collections.Generic;

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

        public virtual void Export(string path, IEnumerable<Apartment> apartments) { 
        }
    }
}
