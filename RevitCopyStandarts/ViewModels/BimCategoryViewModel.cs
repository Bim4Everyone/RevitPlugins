using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitCopyStandarts.ViewModels {
    internal sealed class BimCategoryViewModel {
        private readonly Document _mainDocument;
        private readonly Application _application;

        public BimCategoryViewModel(string categoryName, IEnumerable<FileInfo> files, Document mainDocument, Application application) {
            Name = categoryName;
            
            _mainDocument = mainDocument;
            _application = application;

            BimFiles = new ObservableCollection<BimFileViewModel>(files
                .Select(item => new BimFileViewModel(GetStandard(item), item, _application, _mainDocument))
                .OrderBy(item => item.Name));
        }

        public string Name { get; set; }
        public ObservableCollection<BimFileViewModel> BimFiles { get; set; }

        public string GetStandard(FileInfo fileInfo) {
            return string.Join("_", Path.GetFileNameWithoutExtension(fileInfo.Name).Split('_').Skip(1));
        }
    }
}
