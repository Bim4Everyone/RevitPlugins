using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels {
    internal interface IParametersViewModel : ISelectableElement, INotifyPropertyChanged {
        int Count { get; }
        string DisplayData { get; }
        ObservableCollection<IParameterViewModel> Values { get; }

        IEnumerable<Element> GetSelectedElements();
    }
}