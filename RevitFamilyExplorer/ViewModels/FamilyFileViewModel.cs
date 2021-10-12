using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel {
        private readonly FileInfo _familyFile;

        public FamilyFileViewModel(FileInfo familyFile) {
            _familyFile = familyFile;
        }

        public string Name {
            get { return _familyFile.Name; }
        }
    }
}
