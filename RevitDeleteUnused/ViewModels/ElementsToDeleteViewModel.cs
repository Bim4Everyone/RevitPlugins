using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using Autodesk.Revit.DB;

using RevitDeleteUnused.Models;
using RevitDeleteUnused.Commands;


namespace RevitDeleteUnused.ViewModels {
    internal class ElementsToDeleteViewModel : BaseViewModel {
        ObservableCollection<ElementToDelete> _elementsToDelete;

        public ElementsToDeleteViewModel(Document document, ObservableCollection<ElementToDelete> elementsToDelete, string name) {
            _elementsToDelete = elementsToDelete;
            DeleteSelected = new RelayCommand(obj => { DeleteCommand.DeleteSelectedCommand(document, ElementsToDelete); });
            Name = name;
        }
        public string Name { get; }
        public ObservableCollection<ElementToDelete> ElementsToDelete => _elementsToDelete;
        public ICommand DeleteSelected { get; }
    }
}
