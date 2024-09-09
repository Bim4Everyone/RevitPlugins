using System.Collections.Generic;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    internal abstract class ExportViewModel {
        private readonly string _name;

        protected ExportViewModel(string name) {
            _name = name;
        }

        public string Name => _name;

        public virtual void Export(string path, 
                                   IEnumerable<Apartment> apartments, 
                                   DeclarationSettings settings) { 
        }
    }
}
