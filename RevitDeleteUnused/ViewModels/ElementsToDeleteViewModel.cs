using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using Autodesk.Revit.DB;

using RevitDeleteUnused.Models;


namespace RevitDeleteUnused.ViewModels {
    internal class ElementsToDeleteViewModel : BaseViewModel {
        private List<ElementToDeleteViewModel> _elementsToDelete;

        private string _errorText;
        private RevitRepository _revitRepository;

        public ElementsToDeleteViewModel(RevitRepository revitRepository, List<ElementToDeleteViewModel> elementsToDelete, string name) {
            _elementsToDelete = elementsToDelete;
            _revitRepository = revitRepository;

            DeleteSelected = new RelayCommand(Delete, CanDelete);
            Name = name;
        }

        public ICommand DeleteSelected { get; }

        public string Name { get; }
        public List<ElementToDeleteViewModel> ElementsToDelete => _elementsToDelete;

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private void Delete(object p) {
            _revitRepository.DeleteSelectedCommand(ElementsToDelete
                                                    .Where(e => e.IsChecked)
                                                    .Select(e => e.Element)
                                                    .ToList());
        }

        private bool CanDelete(object p) {
            if(!ElementsToDelete.Any(x => x.IsChecked)) {
                ErrorText = "Не выбраны элементы.";
                return false;
            }
            ErrorText = "";
            return true;
        }
    }
}
