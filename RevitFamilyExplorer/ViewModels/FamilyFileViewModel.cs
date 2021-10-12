using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitFamilyExplorer.ViewModels {
    internal class FamilyFileViewModel : BaseViewModel {
        private FileInfo _familyFile;

        public FamilyFileViewModel(FileInfo familyFile) {
            _familyFile = familyFile;
        }

        public string Name {
            get { return _familyFile.Name; }
        }

        public void Refresh(FileInfo newFileInfo) {
            _familyFile = newFileInfo;
            RaisePropertyChanged(nameof(Name));
        }
    }
}