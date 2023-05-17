using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using Autodesk.Revit.DB;

using RevitDeleteUnused.Models;
using RevitDeleteUnused.Commands;


namespace RevitDeleteUnused.ViewModels {
    internal class ElementsToDeleteViewModel : BaseViewModel {
        private List<ElementToDelete> _elementsToDelete;

        private string _errorText;

        public ElementsToDeleteViewModel(Document document, List<ElementToDelete> elementsToDelete, string name) {
            _elementsToDelete = elementsToDelete;
            DeleteSelected = new RelayCommand(obj => { DeleteCommand.DeleteSelectedCommand(document, ElementsToDelete); }, CanDelete);
            Name = name;
        }
        public string Name { get; }
        public List<ElementToDelete> ElementsToDelete => _elementsToDelete;
        public ICommand DeleteSelected { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private bool CanDelete(object p) {
            int checkedElements = ElementsToDelete.Where(x => x.IsChecked).Count();
            if(checkedElements == 0) {
                ErrorText = "Не выбраны элементы.";
                return false;
            }
            ErrorText = "";
            return true;
        }
    }
}
