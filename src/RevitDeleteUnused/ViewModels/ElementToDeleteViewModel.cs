using System.ComponentModel;
using System.Runtime.CompilerServices;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitDeleteUnused.Models {
    internal class ElementToDeleteViewModel : BaseViewModel {

        public ElementToDeleteViewModel(string name, Element element, bool isUsed) {
            Name = name;
            Element = element;
            IsUsed = isUsed;
        }

        public string Name { get; set; }
        public Element Element { get; set; }
        public bool IsUsed { get; set; }

        private bool _isChecked;
        public bool IsChecked {
            get { return _isChecked; }
            set {
                _isChecked = value;
                OnPropertyChanged();
            }
        }
    }
}
