using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class FileViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        public FileViewModel() { }
        public FileViewModel(RevitRepository revitRepository, Document doc, Transform transform) {
            _revitRepository = revitRepository;
            Doc = doc;
            Transform = transform;
            Name = _revitRepository.GetDocumentName(Doc);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public Document Doc { get; }
        public Transform Transform { get; }
    }
}
