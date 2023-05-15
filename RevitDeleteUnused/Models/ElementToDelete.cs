using System.ComponentModel;
using System.Runtime.CompilerServices;

using Autodesk.Revit.DB;

namespace RevitDeleteUnused.Models {
    public class ElementToDelete : INotifyPropertyChanged {

        public ElementToDelete(string name, Element element, bool isUsed) {
            Name = name;
            Element = element;
            IsUsed = isUsed;
        }

        public string Name { get; set; }
        public Element Element { get; set; }
        public bool IsUsed { get; set; }

        private bool isChecked;
        public bool IsChecked {
            get { return isChecked; }
            set {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
