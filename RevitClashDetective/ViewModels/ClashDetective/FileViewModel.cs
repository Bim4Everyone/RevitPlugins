using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class FileViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        private bool _isSelected;
        public FileViewModel() { }
        public FileViewModel(RevitRepository revitRepository, Document doc, Transform transform) {
            _revitRepository = revitRepository;
            Doc = doc;
            Transform = transform;
            Name = _revitRepository.GetDocumentName(Doc);
        }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public Document Doc { get; }
        public Transform Transform { get; }
    }

    internal class SelectAllFilesViewModel : FileViewModel {

        public SelectAllFilesViewModel() {
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
            UnselectAllCommand = new RelayCommand(UnselectAll, CanSelectAll);
        }

        public ICommand SelectAllCommand { get; }
        public ICommand UnselectAllCommand { get; }

        private void SelectAll(object p) {
            foreach(var file in p as IEnumerable<FileViewModel>) {
                file.IsSelected = true;
            }
        }

        private void UnselectAll(object p) {
            foreach(var file in p as IEnumerable<FileViewModel>) {
                file.IsSelected = false;
            }
        }

        private bool CanSelectAll(object p) {
            return (p as IEnumerable<FileViewModel>) != null;
        }
    }
}
